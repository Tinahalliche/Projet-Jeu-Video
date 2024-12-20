using V1JeuVideo;
using System.Threading.Tasks;
using System.Xml.Serialization;

class Program
{
    static async Task Main(string[] args) 
    {
        string xml = "Content/GameConfig.xml";
        string xslt = "Content/GameConfig.xsl";
        string html = "Content/GameConfig.html";
        string xsd = "Content/GameConfig.xsd";

      
        Game1.XslTransform(xml, xslt, html);
        await Game1.ValidateXmlFileAsync(xsd, xml);

        string xsltBest = "Content/BestScore.xsl";
        string htmlBest = "Content/BestScore.html";
        Game1.XslTransform(xml, xsltBest, htmlBest);

        using var game = new Game1();
        game.Run();
        Game1.Serialization(game, @"Content/GameConfig.xml");
        Game1.Deserialization<Game1>(@"Content/GameConfig.xml");
    }
}