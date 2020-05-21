using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scribs.Core;
using Scribs.Core.Entities;
using Scribs.Core.Services;
using Scribs.Core.Storages;

namespace Scribs.Console {
    class Program {
        static void Main(string[] args) {
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();
            var services = new ServiceCollection().Configure(configuration).AddServices().AddServices().BuildServiceProvider();

            var user = new User("gdrtf");
            //services.GetFactory<User>().CreateAsync(user).Wait();

            //var gdrtf = services.GetFactory<User>().GetByName("gdrtf");
            //gdrtf.Mail = "test@test.test";
            //services.GetFactory<User>().Update(gdrtf);
            //var project = services.GetService<GitStorage>().Load(user.Name, "jlg");
            //services.GetService<MongoStorage>().Save(project);
            //services.GetService<JsonStorage>().Save(project);
            //project = services.GetService<JsonStorage>().Load(gdrtf.Name, "test");
            //services.GetFactory<Document>().Create(project);


            //var deserializer = new DataContractJsonSerializer(typeof(Document));
            //var m = new MemoryStream(Encoding.UTF8.GetBytes(@"{""Key"":""fezstgert"",""Name"":""test2"",""Index"":""3""}"));
            //var doc = deserializer.ReadObject(m) as Document;
            var pandoc = services.GetService<PandocService>().Convert(@"# 1. Mémoires \#1\r\n... *Ses yeux lançaient des éclairs de haine et il levait son menton comme pour offrir son cou à la bête, geste de défi qui finit de rendre fou de rage son père.Sa joue gonflait, elle s'enflammait par vagues douloureuses.* Et alors ? *Effondré sur le sol, les dents serrées, il retenait ses larmes et fermait ses mains lacérées par les débris d'une bouteille de bière qui venait de se briser en même temps que sa pommette.Ses yeux, eux, continuaient d'afficher leur mépris envers celui qu'il appelait autrefois * Papa * et qu'il surnommait désormais ouvertement* l'Ivrogne. * Un coup de pied le frappa dans les côtes, un craquement résonna jusque dans son crâne alors qu'il était projeté contre la porte d'entrée.Elle lui percuta violemment l'épaule pour le laisser retomber par terre, tête la première dans le tapis d'entrée couvert de boue et de l'excrément de chien que son père avait amené sous sa semelle et essuyé sur le* W *de* Welcome.\r\nEt... Et alors ? *Couché sur le tapis, sans force ni espoir d'échapper au monstre, il était aveuglé par le sang que laissait couler son front.Privé de sa vue, il ne pouvait qu'entendre. Et il sentit dans tout son corps les vibrations imposantes de son père approchant. Pas après pas. Après pas. Après pas. La semelle du monstre écrasa son visage. Sa voix, elle, semblait si lointaine, et pourtant si puissante.*\r\n-*C'est quoi ce regard ? *\r\n * Son père grognait, il grognait de plaisir.*\r\n - *T'as quelque chose à me dire petite merde ? T' as quelque chose à dire à celui qui se saigne au boulot tous les jours pour te payer de quoi bouffer ? *\r\n * Petite - merde avait du mal à comprendre, un sifflement commençait à lui bombarder les oreilles.Il sentit pourtant nettement les trois coups timides donnés à la porte, depuis le palier, trois coups et un long silence.L'enfant espéra, pendant quelques secondes, une éternité. Et de l'autre côté de la porte, une voix se fit entendre.*\r\n - *Bonjour.C'est Tom, euh... Le fils de votre voisine. Tout va bien ? J'entends pas mal de bruit...*\r\n - *À l'aide ! lâcha l'enfant.*\r\n * Il avait concentré toutes ses forces dans cet ultime effort, son père le fit taire en écrasant un peu plus son visage contre le tapis.*\r\n - *Merci mais ce n'est rien, fit-il en s'approchant de la porte.Je déménage juste quelques meubles, j'ai fait tomber une étagère.*\r\n*Le géant écrasait sa progéniture de plus en plus fort. La douleur devenait lointaine. Plusieurs secondes s'écoulèrent, l'enfant priait pour que le voisin défonce la porte.* Il a entendu, *décida-t-il.* Bien-sûr qu'il a entendu.Il a forcément entendu.\r\n - *D'accord ! répondit-il. N'hésitez pas, si vous avez besoin d'un coup de main.*\r\n*L'enfant sentit son esprit se briser.C'était comme s'il avait jusque - là rampé sur une fine couche de glace pour échapper à la bête, mais la glace venait de craquer sous son corps.Il coulait.Il se noyait.Le froid l'enveloppait. Des bruits de pas s'éloignaient sur le palier.Dans une pièce voisine, sa mère s'efforçait de pleurer en silence. Et lui aussi, il pleurait. Il s'en rendait compte maintenant, ces idiotes de larmes coulaient sur ses joues.*\r\n - *Pitié... supplia - t - il.*\r\n * Le pied de la bête libéra son visage pour le faire rouler sur le dos avant de s'écraser sur son ventre, enfonçant ses côtes brisées plus profondément dans son thorax. La bête s'accroupit sur ce pied et approcha son visage de celui de l'enfant, qui ne pouvait plus respirer, plus penser, plus bouger, le regard fixé sur le sourire carnassier du monstre et ses crocs terrifiants. Des effluves d'alcool s'échappaient de sa gueule. Ses yeux étaient fous. Exorbités. Assoiffés. Il avait un tesson de bière à la main, l'enfant n'en savait rien, il observait la scène depuis les profondeurs de son esprit, il ne voyait plus rien, il ne sentait plus rien, il coulait, et quelques jours avant son dixième anniversaire, il allait être frappé d'une blessure dont il devrait porter les séquelles toute sa vie...*\r\n\r\n",
                FileType.markdown, FileType.html);

        }
    }
}
