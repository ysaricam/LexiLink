using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Tests.Games.Domain.GameLinks;

public static class DataSet
{
    public static (Link Root, Dictionary<LinkId, Link> AllLinks) Build()
    {
        var links = new Dictionary<LinkId, Link>();

        Link Add(string val, params LinkId[] subIds)
        {
            var link = Link.Of(val, subIds);
            links.Add(link.Id, link);
            return link;
        }

        // 20. SET
        var karaDelik      = Add("Kara Delik");
        var uzayZaman      = Add("Uzay-Zaman");
        var izafiyet       = Add("İzafiyet Teorisi");
        var einstein       = Add("Einstein");
        var deney          = Add("Deney");
        var laboratuvar    = Add("Laboratuvar",        deney.Id, einstein.Id, izafiyet.Id, uzayZaman.Id, karaDelik.Id);

        // 19. SET
        var mikroskop      = Add("Mikroskop");
        var buyutec        = Add("Büyüteç");
        var sherlock       = Add("Sherlock Holmes");
        var polisiye       = Add("Polisiye");
        var agathaChristie = Add("Agatha Christie",    polisiye.Id, sherlock.Id, buyutec.Id, mikroskop.Id, laboratuvar.Id);

        // 18. SET
        var doguEkspresi   = Add("Doğu Ekspresi");
        var tren           = Add("Tren");
        var buharliMakine  = Add("Buharlı Makine");
        var sanayiDevrimi  = Add("Sanayi Devrimi");
        var demirci        = Add("Demirci",            sanayiDevrimi.Id, buharliMakine.Id, tren.Id, doguEkspresi.Id, agathaChristie.Id);

        // 17. SET
        var kilic          = Add("Kılıç");
        var ekskalibur     = Add("Ekskalibur");
        var kralArthur     = Add("Kral Arthur");
        var efsane         = Add("Efsane");
        var kayipKita      = Add("Kayıp Kıta",         efsane.Id, kralArthur.Id, ekskalibur.Id, kilic.Id, demirci.Id);

        // 16. SET
        var atlantis       = Add("Atlantis");
        var okyanus        = Add("Okyanus");
        var kirlilik       = Add("Çevre Kirliliği");
        var geriDonusum    = Add("Geri Dönüşüm");
        var hurdalik       = Add("Hurdalık",           geriDonusum.Id, kirlilik.Id, okyanus.Id, atlantis.Id, kayipKita.Id);

        // 15. SET
        var eskiMetal      = Add("Eski Metal");
        var pas            = Add("Pas");
        var demirOksit     = Add("Demir Oksit");
        var kizilGezegen   = Add("Kızıl Gezegen");
        var mars           = Add("Mars",               kizilGezegen.Id, demirOksit.Id, pas.Id, eskiMetal.Id, hurdalik.Id);

        // 14. SET
        var tesla          = Add("Tesla");
        var otomobil       = Add("Otomobil");
        var benzin         = Add("Benzin");
        var cimBicme       = Add("Çim Biçme Makinesi");
        var bahce          = Add("Bahçe",              cimBicme.Id, benzin.Id, otomobil.Id, tesla.Id, mars.Id);

        // 13. SET
        var lale           = Add("Lale");
        var hollanda       = Add("Hollanda");
        var portakal       = Add("Portakal");
        var cVitamini      = Add("C Vitamini");
        var aciBiber       = Add("Acı Biber",          cVitamini.Id, portakal.Id, hollanda.Id, lale.Id, bahce.Id);

        // 12. SET
        var taco           = Add("Taco");
        var meksika        = Add("Meksika");
        var aztekler       = Add("Aztekler");
        var takvim         = Add("Takvim");
        var gunler         = Add("Haftanın Günleri",   takvim.Id, aztekler.Id, meksika.Id, taco.Id, aciBiber.Id);

        // 11. SET
        var cuma           = Add("Cuma");
        var robinson       = Add("Robinson Crusoe");
        var issizAda       = Add("Issız Ada");
        var ada            = Add("Ada");
        var alcatraz       = Add("Alcatraz",           ada.Id, issizAda.Id, robinson.Id, cuma.Id, gunler.Id);

        // 10. SET
        var hapishane      = Add("Hapishane");
        var daltonlar      = Add("Daltonlar");
        var redKit         = Add("Red Kit");
        var vahsiBati      = Add("Vahşi Batı");
        var kovboy         = Add("Kovboy",             vahsiBati.Id, redKit.Id, daltonlar.Id, hapishane.Id, alcatraz.Id);

        // 9. SET
        var kamci          = Add("Kamçı");
        var indianaJones   = Add("Indiana Jones");
        var arkeoloji      = Add("Arkeoloji");
        var antikCag       = Add("Antik Çağ");
        var mumya          = Add("Mumya",              antikCag.Id, arkeoloji.Id, indianaJones.Id, kamci.Id, kovboy.Id);

        // 8. SET
        var piramitler     = Add("Mısır Piramitleri");
        var nilNehri       = Add("Nil Nehri");
        var timsah         = Add("Timsah");
        var kanalizasyon   = Add("Kanalizasyon");
        var ustaSplinter   = Add("Usta Splinter",      kanalizasyon.Id, timsah.Id, nilNehri.Id, piramitler.Id, mumya.Id);

        // 7. SET
        var ninjaKplmb     = Add("Ninja Kaplumbağalar");
        var pizza          = Add("Pizza");
        var italya         = Add("İtalya");
        var ronesans       = Add("Rönesans");
        var daVinci        = Add("Leonardo da Vinci",  ronesans.Id, italya.Id, pizza.Id, ninjaKplmb.Id, ustaSplinter.Id);

        // 6. SET
        var ressam         = Add("Ressam");
        var sanatOkulu     = Add("Sanat Okulu");
        var hitler         = Add("Adolf Hitler");
        var dunyaSavasi    = Add("İkinci Dünya Savaşı");
        var enigma         = Add("Enigma",             dunyaSavasi.Id, hitler.Id, sanatOkulu.Id, ressam.Id, daVinci.Id);

        // 5. SET
        var alanTuring     = Add("Alan Turing");
        var yapayZeka      = Add("Yapay Zeka");
        var kasparov       = Add("Kasparov");
        var sahMat         = Add("Şah Mat");
        var satranc        = Add("Satranç",            sahMat.Id, kasparov.Id, yapayZeka.Id, alanTuring.Id, enigma.Id);

        // 4. SET
        var napolyon       = Add("Napolyon Bonapart");
        var fransa         = Add("Fransa");
        var ozgurukHeykeli = Add("Özgürlük Heykeli");
        var newYork        = Add("New York");
        var abd            = Add("ABD",                newYork.Id, ozgurukHeykeli.Id, fransa.Id, napolyon.Id, satranc.Id);

        // 3. SET
        var harryKane      = Add("Harry Kane");
        var vikingler      = Add("Vikingler");
        var birlesikKr     = Add("Birleşik Krallık");
        var ingiltere      = Add("İngiltere");
        var ingMilliTakimi = Add("İngiltere Milli Takımı", ingiltere.Id, birlesikKr.Id, vikingler.Id, harryKane.Id, abd.Id);

        // 2. SET
        var itaMilli       = Add("İtalya Milli Takımı");
        var lamineYamal    = Add("Lamine Yamal");
        var realMadrid     = Add("Real Madrid");
        var espLigi        = Add("İspanya Milli Ligi");
        var trMilliLigi    = Add("Türkiye Milli Ligi", espLigi.Id, realMadrid.Id, lamineYamal.Id, ingMilliTakimi.Id, itaMilli.Id);

        // 1. SET
        var trSuperLig     = Add("Türkiye Süper Lig");
        var mertHakan      = Add("Mert Hakan");
        var ardaGuler      = Add("Arda Güler");
        var fenerbahce     = Add("Fenerbahçe",         ardaGuler.Id, mertHakan.Id, trSuperLig.Id, trMilliLigi.Id);
    
        return (fenerbahce, links);    
    }
}