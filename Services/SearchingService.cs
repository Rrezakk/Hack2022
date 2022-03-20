using System.Globalization;
using Hack2022.Models;

namespace Hack2022.Services
{
    public class SearchingService
    {
        public SearchingService(){}

        
        public List<EventModel> Search(List<EventModel> events, string query)
        {
            var res = events.Where(n => n.Name.ToLower().StartsWith(query.ToLower())).ToList();
            res.AddRange(events.Where(n => !n.Name.ToLower().StartsWith(query.ToLower())));
            try
            {
                return res.GetRange(0, 5);
            }
            catch (Exception)
            {
                return res;
            }
        }

        public List<string> GetTags(List<EventModel> events, string query)
        {
            var names = events.Select(x => x.Name).ToList();
            var res = names.Where(n => n.ToLower().StartsWith(query.ToLower())).ToList();
            res.AddRange(names.Where(n => !n.ToLower().StartsWith(query.ToLower())));
            try
            {
                return res.GetRange(0, 5);
            }
            catch (Exception)
            {
                return res;
            }
           
        }
        //public class SearchComparer : IComparer<EventModel>
        //{
        //    public SearchComparer(string searchString)
        //    {
        //        this.searchString = searchString.ToLower(); ;
        //    }

        //    public string searchString = "";
        //    public int Compare(EventModel x, EventModel y)
        //    {
        //        if (ReferenceEquals(x, y))
        //        {
        //            return 0;
        //        }

        //        if (ReferenceEquals(null, y))
        //        {
        //            return 1;
        //        }

        //        if (ReferenceEquals(null, x))
        //        {
        //            return -1;
        //        }

        //        //var c = Math.Round(GetRelevance(x, searchString) - GetRelevance(y, searchString),1);
        //        return -(int)c;
        //    }
        //}
        //public class TagsComparer : IComparer<string>
        //{
        //    public TagsComparer(string searchString)
        //    {
        //        this.searchString = searchString.ToLower();
        //    }

        //    public string searchString = "";
        //    public int Compare(string? x, string? y)
        //    {

        //        var rel1 = GetRelevance(x, searchString);
        //        var rel2 = GetRelevance(y, searchString);
        //        var c = Math.Round(rel1-rel2) ;
        //        var res = (int) c;
        //        Console.WriteLine($"x:{x} y:{y} 1:{rel1} 2:{rel2} res:{c} res:{res}");
        //        return res;
        //    }
        //}

        //public List<EventModel> Search(List<EventModel> events, string query)
        //{
        //    var com = new SearchComparer(query);
        //    events.Sort(com);
        //    return events;
        //}

        //public List<string> GetTags(List<EventModel> events, string query)
        //{
        //    var com = new TagsComparer(query);
        //    var eventTags = events.Select(x => x.Name).ToList();
        //    eventTags.Sort(com);
        //    return eventTags.ToList();
        //}

        //public static double GetRelevance(EventModel x,string query)
        //{
        //    var s = x.Name.ToLower();
        //    var d = x.Desc.ToLower();
        //    var sx = s.Split(' ').Select(TrimWord).ToList();
        //    var dx = d.Split(' ').Select(TrimWord).ToList();
        //    var qx = query.Split(' ').Select(TrimWord).ToList();

        //    var aRel = Rel(qx, sx);
        //    var bRel = Rel(qx, dx);


        //    var res = (0.2 * bRel*100 + 0.8 * aRel * 100);

        //    if (s.StartsWith(query))
        //    {
        //        res += 10;
        //    }
        //    return res;
        //} 
        //public static double GetRelevance(string obj,string query)
        //{
        //    var s = obj;
        //    var sx = s.Split(' ').Select(TrimWord).ToList();
        //    var qx = query.Split(' ').Select(TrimWord).ToList();

        //    var aRel = Rel(qx, sx);
        //    Console.WriteLine($"{aRel}");
        //    if (s.StartsWith(query.ToLower()))
        //    {
        //        aRel += 10;
        //    }
        //    return -aRel;
        //}
        //private static double Rel(List<string> query,List<string> elements)
        //{
        //    query.Select(q => q.ToLower());
        //    elements.Select(q => q.ToLower());
        //    var allctr = elements.Count;
        //    var tCtr = query.Sum(q => elements.Count());
        //    return (double)((double)tCtr / (double)allctr);
        //}
        //public static string TrimWord(string word)
        //{
        //    var finlen = 0;
        //    var len = word.Length;
        //    finlen = len switch
        //    {
        //        <= 7 and > 4 => (int) Math.Round(len * 0.75),
        //        <= 4 => (int) Math.Round(len * 0.85),
        //        _ => (int) Math.Round(len * 0.69)
        //    };
        //    return word.ToLower()[..finlen];
        //}
    }
}
