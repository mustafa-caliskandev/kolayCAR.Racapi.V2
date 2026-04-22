const searchInput = document.querySelector('#docs-search');
const sections = [...document.querySelectorAll('.doc-section')];
const sidebarLinks = [...document.querySelectorAll('.sidebar a[href^="#"]')];
const topLinks = [...document.querySelectorAll('.topnav a[href^="#"]')];
const tocLinks = [...document.querySelectorAll('.toc a[href^="#"]')];
const endpointList = document.querySelector('#endpoint-list');
const endpointCount = document.querySelector('#endpoint-count');
const tokenInput = document.querySelector('#bearer-token');
const authUsernameInput = document.querySelector('#auth-username');
const authPasswordInput = document.querySelector('#auth-password');
const authSecretInput = document.querySelector('#auth-secret');
const authResponseOutput = document.querySelector('#auth-response');
const sendAuthButton = document.querySelector('#send-auth');
const clearAuthButton = document.querySelector('#clear-auth');
const saveTokenButton = document.querySelector('#save-token');
const clearTokenButton = document.querySelector('#clear-token');
const toast = document.querySelector('#toast');
const tokenStorageKey = 'kolaycar.docs.bearerToken';
let openApiDocument = null;
let operations = [];
let selectedMethod = 'all';

function showToast(message) {
  toast.textContent = message;
  toast.classList.add('is-visible');
  window.setTimeout(() => toast.classList.remove('is-visible'), 1500);
}

function setActive(hash) {
  const activate = link => link.classList.toggle('is-active', link.getAttribute('href') === hash);
  sidebarLinks.forEach(activate);
  topLinks.forEach(activate);
  tocLinks.forEach(activate);
}

const observer = new IntersectionObserver(entries => {
  const visible = entries
    .filter(entry => entry.isIntersecting)
    .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];

  if (visible) {
    setActive(`#${visible.target.id}`);
  }
}, { rootMargin: '-22% 0px -64% 0px', threshold: [0.12, 0.3, 0.5] });
sections.forEach(section => observer.observe(section));

window.addEventListener('keydown', event => {
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
    event.preventDefault();
    searchInput.focus();
  }
});

searchInput.addEventListener('input', event => {
  const query = event.target.value.trim().toLowerCase();
  sections.forEach(section => {
    section.hidden = query.length > 0 && !section.textContent.toLowerCase().includes(query);
  });
  renderOperations();
});

document.querySelector('[data-copy="origin"]').addEventListener('click', async () => {
  await copyText(window.location.origin);
});

document.querySelectorAll('[data-method]').forEach(button => {
  button.addEventListener('click', () => {
    selectedMethod = button.dataset.method;
    document.querySelectorAll('[data-method]').forEach(item => item.classList.toggle('is-selected', item === button));
    renderOperations();
  });
});

tokenInput.value = localStorage.getItem(tokenStorageKey) || '';
saveTokenButton.addEventListener('click', () => {
  localStorage.setItem(tokenStorageKey, tokenInput.value.trim());
  showToast('Token saved in this browser');
});
clearTokenButton.addEventListener('click', () => {
  tokenInput.value = '';
  localStorage.removeItem(tokenStorageKey);
  showToast('Token cleared');
});

clearAuthButton.addEventListener('click', () => {
  authUsernameInput.value = '';
  authPasswordInput.value = '';
  authSecretInput.value = '';
  authResponseOutput.textContent = 'Token response will appear here.';
  authResponseOutput.classList.remove('is-error');
});

sendAuthButton.addEventListener('click', sendAuthRequest);

function findToken(value) {
  if (!value || typeof value !== 'object') return null;

  for (const key of ['token', 'Token', 'accessToken', 'AccessToken', 'access_token']) {
    if (typeof value[key] === 'string' && value[key].trim()) {
      return value[key].trim();
    }
  }

  for (const nestedValue of Object.values(value)) {
    const token = findToken(nestedValue);
    if (token) return token;
  }

  return null;
}

async function sendAuthRequest() {
  const payload = {
    username: authUsernameInput.value.trim(),
    password: authPasswordInput.value,
    secretKey: authSecretInput.value.trim()
  };

  authResponseOutput.classList.remove('is-error');
  authResponseOutput.textContent = 'Requesting token...';
  sendAuthButton.disabled = true;

  try {
    const startedAt = performance.now();
    const response = await fetch('/users/authenticate', {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });
    const elapsed = Math.round(performance.now() - startedAt);
    const text = await response.text();
    let parsed = null;
    let body = text || '(empty response)';

    try {
      parsed = text ? JSON.parse(text) : null;
      if (parsed) body = JSON.stringify(parsed, null, 2);
    } catch {
      parsed = null;
    }

    const token = findToken(parsed);
    if (token) {
      tokenInput.value = token;
      localStorage.setItem(tokenStorageKey, token);
      showToast('Token received and saved');
    }

    authResponseOutput.textContent = `HTTP ${response.status} ${response.statusText} (${elapsed} ms)\nPOST /users/authenticate\n\n${body}`;
    authResponseOutput.classList.toggle('is-error', !response.ok || !token);
  } catch (error) {
    authResponseOutput.textContent = `Token request failed:\n${error.message}`;
    authResponseOutput.classList.add('is-error');
  } finally {
    sendAuthButton.disabled = false;
  }
}
async function copyText(text) {
  try {
    await navigator.clipboard.writeText(text);
    showToast(`Copied ${text}`);
  } catch {
    showToast(text);
  }
}

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
}

function methodClass(method) {
  if (method === 'GET') return 'get';
  if (method === 'POST') return 'post';
  return 'other';
}

function operationTitle(operation, path) {
  return operation.summary || operation.operationId || path;
}

function operationDescription(operation) {
  return operation.description || 'Live operation generated from the OpenAPI document.';
}

function resolveRef(schema) {
  if (!schema || !schema.$ref || !openApiDocument?.components?.schemas) return schema;
  const name = schema.$ref.split('/').pop();
  return openApiDocument.components.schemas[name] || schema;
}

function sampleFromSchema(schema, depth = 0) {
  if (!schema || depth > 4) return null;
  schema = resolveRef(schema);

  if (schema.example !== undefined) return schema.example;
  if (schema.default !== undefined) return schema.default;
  if (schema.enum?.length) return schema.enum[0];
  if (schema.oneOf?.length) return sampleFromSchema(schema.oneOf[0], depth + 1);
  if (schema.anyOf?.length) return sampleFromSchema(schema.anyOf[0], depth + 1);
  if (schema.allOf?.length) {
    return schema.allOf.reduce((result, item) => {
      const sample = sampleFromSchema(item, depth + 1);
      return typeof sample === 'object' && sample && !Array.isArray(sample) ? { ...result, ...sample } : result;
    }, {});
  }

  const type = schema.type || (schema.properties ? 'object' : 'string');
  if (type === 'object') {
    const result = {};
    Object.entries(schema.properties || {}).forEach(([key, value]) => {
      result[key] = sampleFromSchema(value, depth + 1);
    });
    return result;
  }
  if (type === 'array') return [sampleFromSchema(schema.items, depth + 1)];
  if (type === 'integer' || type === 'number') return 0;
  if (type === 'boolean') return true;
  if (schema.format === 'date-time') return new Date().toISOString();
  if (schema.format === 'date') return new Date().toISOString().slice(0, 10);
  return 'string';
}

function requestBodySample(operation) {
  const jsonContent = operation.requestBody?.content?.['application/json'] ||
    operation.requestBody?.content?.['text/json'] ||
    operation.requestBody?.content?.['application/*+json'];

  if (!jsonContent) return '';
  const example = jsonContent.example ?? Object.values(jsonContent.examples || {})[0]?.value;
  const sample = example !== undefined ? example : sampleFromSchema(jsonContent.schema);
  return JSON.stringify(sample ?? {}, null, 2);
}

function parameterPlaceholder(parameter) {
  if (parameter.example !== undefined) return parameter.example;
  const sample = sampleFromSchema(parameter.schema);
  return sample === null || sample === undefined ? '' : sample;
}

function renderQueryInputs(operation) {
  if (!operation.queryParams.length) {
    return '<p class="empty-note">No query parameters documented for this endpoint.</p>';
  }

  return operation.queryParams.map(parameter => `
    <label class="field-row">
      <span>${escapeHtml(parameter.name)}${parameter.required ? ' <b>required</b>' : ''}</span>
      <input data-query-name="${escapeHtml(parameter.name)}" value="" placeholder="${escapeHtml(parameterPlaceholder(parameter))}">
    </label>
  `).join('');
}

function renderBodyInput(operation) {
  if (!operation.sampleBody) return '';
  return `
    <label class="body-field">
      <span>JSON body</span>
      <textarea data-body rows="12" spellcheck="false">${escapeHtml(operation.sampleBody)}</textarea>
    </label>
  `;
}

function renderInlineMarkdown(value) {
  return escapeHtml(value)
    .replace(/`([^`]+)`/g, '<code>$1</code>')
    .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
}

function splitMarkdownTableRow(line) {
  return line.trim().replace(/^\|/, '').replace(/\|$/, '').split('|').map(column => column.trim());
}

function isMarkdownSeparator(line) {
  return /^\s*\|?[\s|:-]+\|\s*$/.test(line);
}

function renderMarkdownTable(lines) {
  const rows = lines.filter(line => !isMarkdownSeparator(line)).map(splitMarkdownTableRow);
  if (!rows.length) return '';

  const header = rows[0];
  const body = rows.slice(1);
  return `
    <div class="markdown-table-wrap">
      <table class="markdown-table">
        <thead><tr>${header.map(column => `<th>${renderInlineMarkdown(column)}</th>`).join('')}</tr></thead>
        <tbody>${body.map(row => `<tr>${row.map(column => `<td>${renderInlineMarkdown(column)}</td>`).join('')}</tr>`).join('')}</tbody>
      </table>
    </div>
  `;
}

function markdownToHtml(markdown) {
  if (!markdown?.trim()) return '<p class="empty-note">No details documented for this endpoint.</p>';

  const lines = markdown.replace(/\r\n/g, '\n').replace(/\r/g, '\n').split('\n');
  const html = [];
  let paragraph = [];
  let table = [];
  let code = [];
  let inCode = false;

  const flushParagraph = () => {
    if (!paragraph.length) return;
    html.push(`<p>${renderInlineMarkdown(paragraph.join(' '))}</p>`);
    paragraph = [];
  };
  const flushTable = () => {
    if (!table.length) return;
    html.push(renderMarkdownTable(table));
    table = [];
  };

  for (const line of lines) {
    if (line.trim().startsWith('```')) {
      if (inCode) {
        html.push(`<pre class="markdown-code"><code>${escapeHtml(code.join('\n'))}</code></pre>`);
        code = [];
        inCode = false;
      } else {
        flushParagraph();
        flushTable();
        inCode = true;
      }
      continue;
    }

    if (inCode) {
      code.push(line);
      continue;
    }

    if (line.trim().startsWith('|')) {
      flushParagraph();
      table.push(line);
      continue;
    }

    flushTable();

    const heading = line.match(/^(#{2,4})\s+(.+)$/);
    if (heading) {
      flushParagraph();
      const level = heading[1].length + 1;
      html.push(`<h${level}>${renderInlineMarkdown(heading[2])}</h${level}>`);
      continue;
    }

    if (!line.trim()) {
      flushParagraph();
      continue;
    }

    paragraph.push(line.trim());
  }

  flushParagraph();
  flushTable();
  if (inCode) {
    html.push(`<pre class="markdown-code"><code>${escapeHtml(code.join('\n'))}</code></pre>`);
  }

  return html.join('');
}
function renderOperations() {
  const query = searchInput.value.trim().toLowerCase();
  const filtered = operations.filter(operation => {
    const methodMatches = selectedMethod === 'all' || operation.method === selectedMethod;
    const searchText = `${operation.method} ${operation.path} ${operation.title} ${operation.description}`.toLowerCase();
    return methodMatches && (!query || searchText.includes(query));
  });

  if (filtered.length === 0) {
    endpointList.innerHTML = '<article class="operation-card"><div class="operation-main"><span class="method other">INFO</span><div class="operation-path"><code>No matching endpoints found.</code><small>Try another search term or filter.</small></div></div></article>';
    return;
  }

  endpointList.innerHTML = filtered.map(operation => `
    <article class="operation-card" data-operation-id="${operation.id}">
      <div class="operation-main">
        <span class="method ${methodClass(operation.method)}">${operation.method}</span>
        <div class="operation-path">
          <code>${escapeHtml(operation.path)}</code>
          <small>${escapeHtml(operation.title)}</small>
        </div>
        <div class="operation-actions">
          <button class="copy-endpoint" type="button" data-copy-operation="${operation.id}">Copy</button>
          <button class="copy-endpoint" type="button" data-toggle-details="${operation.id}">Details</button>
          <button class="try-button" type="button" data-toggle-try="${operation.id}">Test</button>
        </div>
      </div>
      <div class="details-panel" data-details-panel="${operation.id}" hidden>${markdownToHtml(operation.description)}</div>

      <div class="try-panel" data-try-panel="${operation.id}" hidden>
        <div class="try-warning">This request runs against the current server. POST requests may create or change real data.</div>
        <label class="field-row full">
          <span>Request URL</span>
          <input data-request-path value="${escapeHtml(operation.path)}">
        </label>
        <div class="query-fields">${renderQueryInputs(operation)}</div>
        ${renderBodyInput(operation)}
        <div class="try-actions">
          <button class="send-button" type="button" data-send-operation="${operation.id}">Send request</button>
          <button class="small-button muted" type="button" data-reset-operation="${operation.id}">Reset</button>
        </div>
        <pre class="response-output" data-response-output="${operation.id}">Response will appear here.</pre>
      </div>
    </article>
  `).join('');

  endpointList.querySelectorAll('[data-copy-operation]').forEach(button => {
    button.addEventListener('click', () => {
      const operation = operations.find(item => item.id === button.dataset.copyOperation);
      copyText(`${operation.method} ${operation.path}`);
    });
  });

  endpointList.querySelectorAll('[data-toggle-details]').forEach(button => {
    button.addEventListener('click', () => {
      const panel = endpointList.querySelector(`[data-details-panel="${button.dataset.toggleDetails}"]`);
      panel.hidden = !panel.hidden;
      button.textContent = panel.hidden ? 'Details' : 'Hide details';
    });
  });

  endpointList.querySelectorAll('[data-toggle-try]').forEach(button => {
    button.addEventListener('click', () => {
      const panel = endpointList.querySelector(`[data-try-panel="${button.dataset.toggleTry}"]`);
      panel.hidden = !panel.hidden;
      button.textContent = panel.hidden ? 'Test' : 'Close';
    });
  });

  endpointList.querySelectorAll('[data-reset-operation]').forEach(button => {
    button.addEventListener('click', () => renderOperations());
  });

  endpointList.querySelectorAll('[data-send-operation]').forEach(button => {
    button.addEventListener('click', () => sendOperation(button.dataset.sendOperation));
  });
}

function buildRequestUrl(card) {
  const pathInput = card.querySelector('[data-request-path]');
  const rawPath = pathInput.value.trim() || '/';
  const url = new URL(rawPath.startsWith('http') ? rawPath : rawPath, window.location.origin);

  card.querySelectorAll('[data-query-name]').forEach(input => {
    const value = input.value.trim();
    if (value) url.searchParams.set(input.dataset.queryName, value);
  });

  return url;
}

async function sendOperation(operationId) {
  const operation = operations.find(item => item.id === operationId);
  const card = endpointList.querySelector(`[data-operation-id="${operationId}"]`);
  const output = card.querySelector('[data-response-output]');
  const sendButton = card.querySelector('[data-send-operation]');
  const bodyInput = card.querySelector('[data-body]');
  const headers = { Accept: 'application/json' };
  const bearerToken = tokenInput.value.trim();

  if (bearerToken) {
    headers.Authorization = bearerToken.toLowerCase().startsWith('bearer ') ? bearerToken : `Bearer ${bearerToken}`;
  }

  const options = { method: operation.method, headers };
  if (bodyInput && operation.method !== 'GET') {
    const body = bodyInput.value.trim();
    if (body) {
      headers['Content-Type'] = 'application/json';
      options.body = body;
    }
  }

  let url;
  try {
    url = buildRequestUrl(card);
    if (options.body) JSON.parse(options.body);
  } catch (error) {
    output.textContent = `Request could not be prepared:\n${error.message}`;
    output.classList.add('is-error');
    return;
  }

  output.classList.remove('is-error');
  output.textContent = 'Sending request...';
  sendButton.disabled = true;

  const startedAt = performance.now();
  try {
    const response = await fetch(url, options);
    const elapsed = Math.round(performance.now() - startedAt);
    const text = await response.text();
    const contentType = response.headers.get('content-type') || '';
    let body = text;

    if (contentType.includes('application/json') && text) {
      body = JSON.stringify(JSON.parse(text), null, 2);
    }

    output.textContent = `HTTP ${response.status} ${response.statusText} (${elapsed} ms)\n${url}\n\n${body || '(empty response)'}`;
    output.classList.toggle('is-error', !response.ok);
  } catch (error) {
    output.textContent = `Request failed:\n${error.message}`;
    output.classList.add('is-error');
  } finally {
    sendButton.disabled = false;
  }
}

async function loadEndpoints() {
  try {
    const response = await fetch('/swagger/v1/swagger.json', { headers: { accept: 'application/json' } });
    if (!response.ok) throw new Error(`OpenAPI returned ${response.status}`);
    openApiDocument = await response.json();
    const methods = ['get', 'post', 'put', 'delete', 'patch', 'head', 'options', 'trace'];

    operations = [];
    Object.entries(openApiDocument.paths || {}).forEach(([path, pathItem]) => {
      Object.entries(pathItem || {}).forEach(([method, operation]) => {
        if (!methods.includes(method)) return;
        const normalizedMethod = method.toUpperCase();
        const parameters = [...(pathItem.parameters || []), ...(operation.parameters || [])];
        operations.push({
          id: `${normalizedMethod}-${path}`.replace(/[^a-z0-9]+/gi, '-'),
          method: normalizedMethod,
          path,
          title: operationTitle(operation || {}, path),
          description: operationDescription(operation || {}),
          queryParams: parameters.filter(parameter => parameter.in === 'query'),
          sampleBody: requestBodySample(operation || {})
        });
      });
    });

    operations.sort((a, b) => `${a.path} ${a.method}`.localeCompare(`${b.path} ${b.method}`));
    endpointCount.textContent = operations.length.toString();
    renderOperations();
  } catch {
    endpointList.innerHTML = '<article class="operation-card"><div class="operation-main"><span class="method other">INFO</span><div class="operation-path"><code>OpenAPI endpoint list could not be loaded.</code><small>Check /swagger/v1/swagger.json.</small></div></div></article>';
  }
}

loadEndpoints();