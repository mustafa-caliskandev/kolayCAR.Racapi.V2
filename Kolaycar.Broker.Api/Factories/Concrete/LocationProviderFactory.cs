using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
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
using EnterpriseProvider = KolayCAR.Broker.API.Providers.Enterprise;
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
using Pandora2Provider = KolayCAR.Broker.API.Providers.Pandora2;
using ReservawayProvider = KolayCAR.Broker.API.Providers.Reservaway;
using RenteonProvider = KolayCAR.Broker.API.Providers.Renteon;
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
using RentGoProvider = Kolaycar.Broker.Api.Providers.RentGo;
using Yolcu360Provider = KolayCAR.Broker.API.Providers.Yolcu360;
using Yolcu360v2Provider = KolayCAR.Broker.API.Providers.Yolcu360v2;
using ZiraatFiloProvider = KolayCAR.Broker.API.Providers.ZiraatFilo;

namespace KolayCAR.Broker.API.Factories.Concrete
{
    public class LocationProviderFactory : ILocationProviderFactory
    {
        public ILocationProvider? CreateLocationProvider(Vendor mappedVendor, Models.Vendor vendor, IMemoryCache _memoryCache, ICacheService _cacheService)
        {
            return mappedVendor.VendorType switch
            {
                VendorTypes.KolayCAR => new KolayCARProvider.LocationProvider(mappedVendor),
                VendorTypes.FiloNova => new FiloNovaProvider.LocationProvider(vendor.Apibaseurl, _memoryCache),
                VendorTypes.KolayCARBroker => new KolayCARBrokerProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Renticar => new RenticarProvider.LocationProvider(vendor.Apibaseurl, _memoryCache),
                VendorTypes.Sixt2 => new Sixt2Provider.LocationProvider(vendor.Apibaseurl, _cacheService),
                VendorTypes.Europcar => new EuropcarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Avec => new AvecProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Ayes => new AyesProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Pandora => new PandoraProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Central => new CentralProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Nissa => new NissaProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Rigorent => new RigorentProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Aytu => new AytuProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Ekar => new EkarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Hara => new HaraProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Elitcar => new ElitcarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Cizgi => new CizgiProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Avec2 => new Avec2Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Wishcar => new WishcarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Otocar => new OtocarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.GreenMotion => new GreenMotionProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Assist => new AssistProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Garenta => new GarentaProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.CredyCar => new CredyCarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Erboycar => new ErboycarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Enterprise => new EnterpriseProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Circular => new CircularProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Akkor => new AkkorProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Dailydrive => new DailydriveProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Turmobil => new TurmobilProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Elibol => new ElibolProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.YDZ => new YdzProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Central2 => new Central2Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.BesS => new BesSProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Beto => new BetoProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Turevrac => new TurevracProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Sixt => new SixtProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Nissa2 => new Nissa2Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Avec3 => new Avec3Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Ekar2 => new Ekar2Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Circular2 => new Circular2Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.AutoHome => new AutoHomeProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Ototur => new OtoturProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Otorento => new OtorentoProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Avis => new AvisProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Turevrac2 => new Turevrac2Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Renteon => new RenteonProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Wheelsys => new WheelsysProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Yolcu360 => new Yolcu360Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.EnUygun => new EnuygunProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.ZiraatFilo => new ZiraatFiloProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Garajlar => new GarajlarProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Yolcu360v2 => new Yolcu360v2Provider.LocationProvider(vendor.Apibaseurl, _cacheService),
                VendorTypes.Eganis => new EganisProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.YesOto => new YesOtoProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Eren => new ErenProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.RentGo => new RentGoProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Vonarent => new VonarentProvider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Pandora2 => new Pandora2Provider.LocationProvider(vendor.Apibaseurl),
                VendorTypes.Reservaway => new ReservawayProvider.LocationProvider(vendor.Apibaseurl),
                _ => null
            };
        }
    }
}
