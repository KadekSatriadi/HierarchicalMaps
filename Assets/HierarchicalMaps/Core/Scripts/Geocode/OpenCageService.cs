using System.IO;
using System.Net;
using System.Xml;

public class OpenCageService
{
    public string API_KEY = "";

    /// <summary>
    /// Return JSON string of reverse geo coding
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public string ReverseGeocodingResponseXML(string latitude, string longitude)
    {
        string json = null;

        string reqstring = "https://api.opencagedata.com/geocode/v1/xml?q="+latitude+"+"+longitude+"&key=" + API_KEY;
        WebRequest req = WebRequest.Create(reqstring);
        //Debug.Log(reqstring);
        HttpWebResponse response = (HttpWebResponse)req.GetResponse();
        //Debug.Log(response.StatusDescription);
        Stream dataStream = response.GetResponseStream();
        StreamReader reader = new StreamReader(dataStream);

        json = reader.ReadToEnd();

        return json;
    }

    /// <summary>
    /// Return the summary of the geo coding
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public  string ReverseGeocoding(string latitude, string longitude)
    {
        string j = ReverseGeocodingResponseXML(latitude, longitude);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(j);

        string result = "";

        XmlNode node = doc.SelectSingleNode("/response/results/result");
        foreach(XmlNode n in node.ChildNodes)
        {
            if (n.Name == "formatted")
            {
                result = n.InnerText;
                break;
            }
        }

        return result;
    }

}
