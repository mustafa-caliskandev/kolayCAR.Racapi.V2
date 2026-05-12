using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services;
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
using ErenProvider = Kolaycar.Broker.Api.Providers.Eren;
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
    public class ReservationProviderFactory : IReservationProviderFactory
    {
        public IReservationProvider CreateReservationProvider(Vendor vendor, IConfigurationService configurationService, IConfiguration configuration, IMemoryCache memoryCache, ICacheService cacheService)
        {
            return vendor.VendorType switch
            {
                VendorTypes.KolayCAR => new KolayCARProvider.ReservationProvider(vendor, configurationService),
                VendorTypes.Avec => new AvecProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.KolayCARBroker => new KolayCARBrokerProvider.ReservationProvider(vendor.APIBaseUrl, cacheService, configurationService, configuration),
                VendorTypes.Aytu => new AytuProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Europcar => new EuropcarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Pandora => new PandoraProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Central => new CentralProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Nissa => new NissaProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Ayes => new AyesProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Rigorent => new RigorentProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.GreenMotion => new GreenMotionProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Ekar => new EkarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Hara => new HaraProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.FiloNova => new FiloNovaProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Elitcar => new ElitcarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Cizgi => new CizgiProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Avec2 => new Avec2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Wishcar => new WishcarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Otocar => new OtocarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Assist => new AssistProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Elibol => new ElibolProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Garenta => new GarentaProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.CredyCar => new CredyCarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Erboycar => new ErboycarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Circular => new CircularProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Turmobil => new TurmobilProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Akkor => new AkkorProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Dailydrive => new DailydriveProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Yolcu360 => new Yolcu360Provider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.BesS => new BesSProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Central2 => new Central2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Beto => new BetoProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Turevrac => new TurevracProvider.ReservationProvider(vendor.APIBaseUrl, configurationService, configuration),
                VendorTypes.Sixt => new SixtProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Renticar => new RenticarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService, memoryCache),
                VendorTypes.Nissa2 => new Nissa2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Avec3 => new Avec3Provider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Ekar2 => new Ekar2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Circular2 => new Circular2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.AutoHome => new AutoHomeProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Ototur => new OtoturProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Otorento => new OtorentoProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Avis => new AvisProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.YDZ => new YdzProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Turevrac2 => new Turevrac2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService, configuration),
                VendorTypes.Renteon => new RenteonProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Wheelsys => new WheelsysProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.EnUygun => new EnuygunProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Sixt2 => new Sixt2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService, cacheService),
                VendorTypes.ZiraatFilo => new ZiraatFiloProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Garajlar => new GarajlarProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Yolcu360v2 => new Yolcu360v2Provider.ReservationProvider(vendor.APIBaseUrl, configurationService, cacheService),
                VendorTypes.Eganis => new EganisProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.YesOto => new YesOtoProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.RentGo => new RentGoProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Eren => new ErenProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                VendorTypes.Vonarent => new VonarentProvider.ReservationProvider(vendor.APIBaseUrl, configurationService),
                _ => null
            };
        }
    }
}
