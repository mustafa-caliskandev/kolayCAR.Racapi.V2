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
    public class VehicleProviderFactory : IVehicleProviderFactory
    {
        public IVehicleProvider CreateVehicleProvider(Vendor vendor, IMemoryCache memoryCache, ICacheService cacheService, bool disableTimeout)
        {
            return vendor.VendorType switch
            {
                VendorTypes.KolayCAR => new KolayCARProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Avec => new AvecProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.KolayCARBroker => new KolayCARBrokerProvider.VehicleProvider(vendor, cacheService, disableTimeout),
                VendorTypes.Aytu => new AytuProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Europcar => new EuropcarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Pandora => new PandoraProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Central => new CentralProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Nissa => new NissaProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Ayes => new AyesProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Rigorent => new RigorentProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.GreenMotion => new GreenMotionProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Ekar => new EkarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Hara => new HaraProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.FiloNova => new FiloNovaProvider.VehicleProvider(vendor, memoryCache, disableTimeout),
                VendorTypes.Elitcar => new ElitcarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Cizgi => new CizgiProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Avec2 => new Avec2Provider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Wishcar => new WishcarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Otocar => new OtocarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Assist => new AssistProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Elibol => new ElibolProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Garenta => new GarentaProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.CredyCar => new CredyCarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Erboycar => new ErboycarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Circular => new CircularProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Turmobil => new TurmobilProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Akkor => new AkkorProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Dailydrive => new DailydriveProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Yolcu360 => new Yolcu360Provider.VehicleProvider(vendor, memoryCache, disableTimeout),
                VendorTypes.BesS => new BesSProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Central2 => new Central2Provider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Beto => new BetoProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Turevrac => new TurevracProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Sixt => new SixtProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Renticar => new RenticarProvider.VehicleProvider(vendor, memoryCache, disableTimeout),
                VendorTypes.Nissa2 => new Nissa2Provider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Avec3 => new Avec3Provider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Ekar2 => new Ekar2Provider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Circular2 => new Circular2Provider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.AutoHome => new AutoHomeProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Ototur => new OtoturProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Otorento => new OtorentoProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Avis => new AvisProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.YDZ => new YdzProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Turevrac2 => new Turevrac2Provider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Renteon => new RenteonProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Wheelsys => new WheelsysProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.EnUygun => new EnuygunProvider.VehicleProvider(vendor, disableTimeout, cacheService),
                VendorTypes.Sixt2 => new Sixt2Provider.VehicleProvider(vendor, cacheService, disableTimeout),
                VendorTypes.ZiraatFilo => new ZiraatFiloProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Garajlar => new GarajlarProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Yolcu360v2 => new Yolcu360v2Provider.VehicleProvider(vendor, disableTimeout, cacheService),
                VendorTypes.Eganis => new EganisProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.YesOto => new YesOtoProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Eren => new ErenProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.RentGo => new RentGoProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Vonarent => new VonarentProvider.VehicleProvider(vendor, disableTimeout),
                VendorTypes.Pandora2 => new Pandora2Provider.VehicleProvider(vendor, disableTimeout),
                _ => null
            };
        }
    }
}
