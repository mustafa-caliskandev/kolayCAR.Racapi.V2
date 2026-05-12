using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using AkkorProvider = KolayCAR.Broker.API.Providers.Akkor;
using AssistProvider = KolayCAR.Broker.API.Providers.Assist;
using AutoHomeProvider = KolayCAR.Broker.API.Providers.AutoHome;
using Avec2Provider = KolayCAR.Broker.API.Providers.Avec2;
using Avec3Provider = KolayCAR.Broker.API.Providers.Avec3;
using AvecProvider = KolayCAR.Broker.API.Providers.Avec;
using AvisProvider = KolayCAR.Broker.API.Providers.Avis;
using AyesProvider = KolayCAR.Broker.API.Providers.Ayes;
using AytuProvider = KolayCAR.Broker.API.Providers.Aytu;
using BesSProvider = KolayCAR.Broker.API.Providers._5S;
using BetoProvider = KolayCAR.Broker.API.Providers.Beto;
using Central2Provider = KolayCAR.Broker.API.Providers.Central2;
using CentralProvider = KolayCAR.Broker.API.Providers.Central;
using Circular2Provider = KolayCAR.Broker.API.Providers.Circular2;
using CircularProvider = KolayCAR.Broker.API.Providers.Circular;
using CizgiProvider = KolayCAR.Broker.API.Providers.Cizgi;
using CredyCarProvider = KolayCAR.Broker.API.Providers.CredyCar;
using DailydriveProvider = KolayCAR.Broker.API.Providers.Dailydrive;
using EganisProvider = KolayCAR.Broker.API.Providers.Eganis;
using Ekar2Provider = KolayCAR.Broker.API.Providers.Ekar2;
using EkarProvider = KolayCAR.Broker.API.Providers.Ekar;
using ElibolProvider = KolayCAR.Broker.API.Providers.Elibol;
using ElitcarProvider = KolayCAR.Broker.API.Providers.Elitcar;
using EnuygunProvider = KolayCAR.Broker.API.Providers.Enuygun;
using ErboycarProvider = KolayCAR.Broker.API.Providers.Erboycar;
using ErenProvider = KolayCAR.Broker.API.Providers.Eren;
using EuropcarProvider = KolayCAR.Broker.API.Providers.Europcar;
using FiloNovaProvider = KolayCAR.Broker.API.Providers.FiloNova;
using GarajlarProvider = KolayCAR.Broker.API.Providers.Garajlar;
using GarentaProvider = KolayCAR.Broker.API.Providers.Garenta;
using GreenMotionProvider = KolayCAR.Broker.API.Providers.GreenMotion;
using HaraProvider = KolayCAR.Broker.API.Providers.Hara;
using KolayCARBrokerProvider = KolayCAR.Broker.API.Providers.KolayCARBroker;
using KolayCARProvider = KolayCAR.Broker.API.Providers.KolayCAR;
using Nissa2Provider = KolayCAR.Broker.API.Providers.Nissa2;
using NissaProvider = KolayCAR.Broker.API.Providers.Nissa;
using OtocarProvider = KolayCAR.Broker.API.Providers.Otocar;
using OtorentoProvider = KolayCAR.Broker.API.Providers.Otorento;
using OtoturProvider = KolayCAR.Broker.API.Providers.Ototur;
using PandoraProvider = KolayCAR.Broker.API.Providers.Pandora;
using RenteonProvider = KolayCAR.Broker.API.Providers.Renteon;
using RentGoProvider = Kolaycar.Broker.Api.Providers.RentGo;
using RenticarProvider = KolayCAR.Broker.API.Providers.Renticar;
using RigorentProvider = KolayCAR.Broker.API.Providers.Rigorent;
using Sixt2Provider = KolayCAR.Broker.API.Providers.Sixt2;
using SixtProvider = KolayCAR.Broker.API.Providers.Sixt;
using Turevrac2Provider = KolayCAR.Broker.API.Providers.Turevrac2;
using TurevracProvider = KolayCAR.Broker.API.Providers.Turevrac;
using TurmobilProvider = KolayCAR.Broker.API.Providers.Turmobil;
using VonarentProvider = KolayCAR.Broker.API.Providers.Vonarent;
using WheelsysProvider = KolayCAR.Broker.API.Providers.Wheelsys;
using WishcarProvider = KolayCAR.Broker.API.Providers.Wishcar;
using YdzProvider = KolayCAR.Broker.API.Providers.Ydz;
using YesOtoProvider = Kolaycar.Broker.Api.Providers.YesOto;
using Yolcu360Provider = KolayCAR.Broker.API.Providers.Yolcu360;
using Yolcu360v2Provider = KolayCAR.Broker.API.Providers.Yolcu360v2;
using ZiraatFiloProvider = KolayCAR.Broker.API.Providers.ZiraatFilo;

namespace KolayCAR.Broker.API.Factories.Concrete
{
    public class ExtraProviderFactory : IExtraProviderFactory
    {
        public IExtraProvider CreateExtraProvider(Vendor vendor, IMemoryCache memoryCache, ICacheService cacheService, IConfiguration configuration)
        {
            return vendor.VendorType switch
            {
                VendorTypes.KolayCAR => new KolayCARProvider.ExtraProvider(vendor),
                VendorTypes.Avec => new AvecProvider.ExtraProvider(),
                VendorTypes.KolayCARBroker => new KolayCARBrokerProvider.ExtraProvider(vendor.APIBaseUrl, cacheService, configuration),
                VendorTypes.Aytu => new AytuProvider.ExtraProvider(),
                VendorTypes.Europcar => new EuropcarProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Pandora => new PandoraProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Central => new CentralProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Nissa => new NissaProvider.ExtraProvider(),
                VendorTypes.Ayes => new AyesProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Rigorent => new RigorentProvider.ExtraProvider(),
                VendorTypes.GreenMotion => new GreenMotionProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Ekar => new EkarProvider.ExtraProvider(),
                VendorTypes.Hara => new HaraProvider.ExtraProvider(),
                VendorTypes.FiloNova => new FiloNovaProvider.ExtraProvider(vendor.APIBaseUrl, memoryCache),
                VendorTypes.Elitcar => new ElitcarProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Cizgi => new CizgiProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Avec2 => new Avec2Provider.ExtraProvider(),
                VendorTypes.Wishcar => new WishcarProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Otocar => new OtocarProvider.ExtraProvider(),
                VendorTypes.Assist => new AssistProvider.ExtraProvider(),
                VendorTypes.Elibol => new ElibolProvider.ExtraProvider(),
                VendorTypes.Garenta => new GarentaProvider.ExtraProvider(vendor),
                VendorTypes.CredyCar => new CredyCarProvider.ExtraProvider(),
                VendorTypes.Erboycar => new ErboycarProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Circular => new CircularProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Turmobil => new TurmobilProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Akkor => new AkkorProvider.ExtraProvider(),
                VendorTypes.Dailydrive => new DailydriveProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Yolcu360 => new Yolcu360Provider.ExtraProvider(vendor.APIBaseUrl, memoryCache),
                VendorTypes.BesS => new BesSProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Central2 => new Central2Provider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Beto => new BetoProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Turevrac => new TurevracProvider.ExtraProvider(),
                VendorTypes.Sixt => new SixtProvider.ExtraProvider(vendor),
                VendorTypes.Renticar => new RenticarProvider.ExtraProvider(vendor, memoryCache),
                VendorTypes.Nissa2 => new Nissa2Provider.ExtraProvider(),
                VendorTypes.Avec3 => new Avec3Provider.ExtraProvider(),
                VendorTypes.Ekar2 => new Ekar2Provider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Circular2 => new Circular2Provider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.AutoHome => new AutoHomeProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Ototur => new OtoturProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Otorento => new OtorentoProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Avis => new AvisProvider.ExtraProvider(),
                VendorTypes.YDZ => new YdzProvider.ExtraProvider(),
                VendorTypes.Turevrac2 => new Turevrac2Provider.ExtraProvider(),
                VendorTypes.Renteon => new RenteonProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Wheelsys => new WheelsysProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.EnUygun => new EnuygunProvider.ExtraProvider(vendor.APIBaseUrl, cacheService),
                VendorTypes.Sixt2 => new Sixt2Provider.ExtraProvider(vendor, cacheService),
                VendorTypes.ZiraatFilo => new ZiraatFiloProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.Garajlar => new GarajlarProvider.ExtraProvider(vendor),
                VendorTypes.Yolcu360v2 => new Yolcu360v2Provider.ExtraProvider(vendor, cacheService),
                VendorTypes.Eganis => new EganisProvider.ExtraProvider(vendor),
                VendorTypes.YesOto => new YesOtoProvider.ExtraProvider(vendor.APIBaseUrl),
                VendorTypes.RentGo => new RentGoProvider.ExtraProvider(),
                VendorTypes.Eren => new ErenProvider.ExtraProvider(vendor),
                VendorTypes.Vonarent => new VonarentProvider.ExtraProvider(vendor),
                _ => null
            };
        }
    }
}
