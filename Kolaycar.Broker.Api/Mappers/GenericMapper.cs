namespace KolayCAR.Broker.API.Mappers
{
    public static class GenericMapper
    {
        //public static List<Ttarget> Map<Tsource, Ttarget, Tmapper>(this List<Tsource> list, string mapMethodName = "Map")
        //    where Tsource : class
        //    where Ttarget : class
        //    where Tmapper : class
        //{
        //    var _targetList = new List<Ttarget>();

        //    if (list != null && list.Count > 0)
        //        foreach (var item in list)
        //        {
        //            var mappedItem = item.Map<Tsource, Ttarget, Tmapper>(mapMethodName);
        //            if (mappedItem != null)
        //                _targetList.Add(mappedItem);
        //        }

        //    return _targetList;
        //}

        //public static Ttarget Map<Tsource, Ttarget, Tmapper>(this Tsource obj, string mapMethodName = "Map")
        //{
        //    var mapMethod = typeof(Tmapper).GetMethod(mapMethodName);
        //    if (mapMethod != null)
        //        return (Ttarget)mapMethod.Invoke(null, new object[] { obj });
        //    else
        //        return default(Ttarget);
        //}
    }
}
