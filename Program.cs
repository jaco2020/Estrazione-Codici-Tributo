using System.Text;
using HtmlAgilityPack;
using System.Xml;
using System;
using System.Net;
using System.Linq;
using System.IO.Compression;

var client = new HttpClient();
string s = null;
string sint = null;
var request = new HttpRequestMessage
{
    Method = HttpMethod.Get,
    RequestUri = new Uri("https://www1.agenziaentrate.gov.it/servizi/codici/ricerca/elencoTributi.php?Q1=Tutte&Q3=Tutte&Q2=Tutte&Q4=Tutte&pRet=MenuQ4.php%3FQ1%3DTutte%26Q3%3DTutte%26Q2%3DTutte"),
};
using (var response = await client.SendAsync(request))
{
    response.EnsureSuccessStatusCode();
    using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync(), Encoding.GetEncoding("iso-8859-1")))
    {
        s = sr.ReadToEnd();
    }
}
//crea la cartella --------- INSERIRE IL PERCORSO DELLA CARTELLA DA CREARE
var dirPath = @"C:\Users\ja-co\Documents\Tutti Html Codici Tributo";
System.IO.Directory.CreateDirectory(dirPath);
//crea il csv --------- INSERIRE IL PERCORSO 
using (StreamWriter sw = new StreamWriter(@"C:\Users\ja-co\Documents\html.csv"))
{
    sw.WriteLine("Tipo Contribuente" + ";" + "Tipo Imposta" + ";" + "Contesto d uso" + ";" + "Tipo Adempimento" + ";" + "Descrizione" + ";" + "Codice Tributo" + ";" + "link" + ";" + "Sezione modello F24 da compilare");
    var htmlDoc = new HtmlDocument();
    htmlDoc.LoadHtml(s);
    string str = "";
    string TipoContribuente = "";
    string TipoImposta = "";
    string Contestouso = "";
    string TipoAdempimento = "";
    string Descrizione = "";
    string CodTributo = "";
    string link = "";
    int a = 0;
    if (htmlDoc.DocumentNode.SelectNodes("//div[@class='table-responsive']").ToArray() != null)
    {
        var table = htmlDoc.DocumentNode.SelectNodes("//div[@class='table-responsive']");
        int bb = 0;
        HtmlNode[] aNodes2 = htmlDoc.DocumentNode.SelectNodes("//div[@class='table-responsive']//tbody/tr").ToArray();
        foreach (var item in aNodes2)
        {
            TipoContribuente = "";
            TipoImposta = "";
            Contestouso = "";
            TipoAdempimento = "";
            Descrizione = "";
            CodTributo = "";
            link = "";
            if (htmlDoc.DocumentNode.SelectNodes(".//td[1]") != null) // TipoContribuente
            {
                TipoContribuente = item.SelectSingleNode(".//td[1]").InnerText.Replace("&nbsp;", "").Replace(";", "");
            }
            if (htmlDoc.DocumentNode.SelectNodes("//td[2]") != null) // TipoImposta
            {
                TipoImposta = item.SelectSingleNode(".//td[2]").InnerText.Replace("&nbsp;", "").Replace(";", "");
            }
            if (htmlDoc.DocumentNode.SelectNodes(".//td[3]") != null) // Contestouso
            {
                Contestouso = item.SelectSingleNode(".//td[3]").InnerText.Replace("&nbsp;", "").Replace(";", "");
            }
            if (htmlDoc.DocumentNode.SelectNodes(".//td[4]") != null) // TipoAdempimento
            {
                TipoAdempimento = item.SelectSingleNode(".//td[4]").InnerText.Replace("&nbsp;", "").Replace(";", "");
            }
            if (htmlDoc.DocumentNode.SelectNodes(".//td[5]") != null) // Descrizione
            {
                Descrizione = item.SelectSingleNode(".//td[5]").InnerText.Replace("&nbsp;", "").Replace(";", "");
            }
            if (htmlDoc.DocumentNode.SelectNodes(".//td[6]") != null) // CodTributo
            {
                CodTributo = item.SelectSingleNode(".//td[@id='tributo']").InnerText.Replace("&nbsp;", "").Replace(";", "");
            }
            if (htmlDoc.DocumentNode.SelectNodes(".//td/a") != null) // link
            {
                link = item.SelectSingleNode(".//td/a").GetAttributeValue("href", "");
            }
            //CHIAMATA ALLE PAGINE INTERNE
            request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://www1.agenziaentrate.gov.it/servizi/codici/ricerca/" + link),
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync(), Encoding.GetEncoding("iso-8859-1")))
                {
                    sint = sr.ReadToEnd();
                }
            }
            var htmlDoc1 = new HtmlDocument();
            htmlDoc1.LoadHtml(sint);
            string data = "";
            if (htmlDoc1.DocumentNode.SelectSingleNode(".//div[@id='Sez1']/p") != null)
            {
                data = htmlDoc1.DocumentNode.SelectSingleNode(".//div[@id='Sez1']/p").InnerText;
                data = data.Replace("Sezione modello F24 da compilare:", "").Replace("Environment.NewLine", "").Replace("Environment.NewLine", "").Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                for (int i = 0; i < 10; i++) { data = data.Replace("  ", " ").Trim(); }
            }
            sw.WriteLine(TipoContribuente + ";" + TipoImposta + ";" + Contestouso + ";" + TipoAdempimento + ";" + Descrizione + ";" + CodTributo
                + ";" + "https://www1.agenziaentrate.gov.it/servizi/codici/ricerca/" + link + ";" + data);
            using (StreamWriter swrit = new StreamWriter(dirPath +@"\"+ CodTributo +".html"))
            {
                swrit.WriteLine(sint);
            }

            bb += 1;
            if (bb == 10)
            {
                //break;
            }
        }
    }
}
if (File.Exists(dirPath + ".zip"))
{
    File.Delete(dirPath + ".zip"); 
}
ZipFile.CreateFromDirectory(dirPath, dirPath + ".zip");
//File.Delete(dirPath);