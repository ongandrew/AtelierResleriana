using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Nodes;
using Universal.Common;
using Universal.Common.Json;
using Universal.GenerativeAI;

namespace AtelierResleriana.Localization.Utilities
{
    public class EnglishLocalizer : ILocalizer
    {
        public const string DetailedCharacterInformation =
        """
        DETAILED CHARACTER MAPPING (Japanese → English):
        - マルローネ (マリー) → Marlone (Marie) - Energetic alchemist, former "Bomberwoman," fond of using explosives to solve problems
        - シア・ドナースターク (シア) → Schea Donnerstag (Shia) - Gentle noblewoman, Marie's childhood friend with occasionally sharp tongue
        - ミュー・セクスタンス (ミュー) → Mu Seqstanse (Mu) - Free-spirited adventurer who dislikes cold places
        - ルーウェン・フィルニール (ルーウェン) → Ruven Filnir (Ruven) - Spirited young adventurer working in the capital
        - エルフィール・トラウム (エリー) → Elfir Traum (Elie) - Sweet-toothed alchemist who loves cheesecake
        - リリー (リリー) → Lilie (Lilie) - Tomboyish alchemist relearning her craft from scratch
        - ユーディット・フォルトーネ (ユーディー) → Judith Volltone (Judie) - Carefree alchemist researching the Dragon Hourglass
        - ヴィオラート・プラターネ (ヴィオラート) → Viorate Platane (Viorate) - Carrot-loving alchemist from Karotte Village
        - ヴェイン・アウレオルス (ヴェイン) → Vayne Aurelius (Vayne) - Quiet academy student on a journey of self-discovery
        - ロゼリュクス・マイツェン (ロゼ) → Razeluxe Meitzen (Raze) - Reserved swordsman and alchemist with excellent combat skills
        - ロロライナ・フリクセル (ロロナ) → Rorolina Frixell (Rorona) - Cheerful pie-loving alchemist who can be airheaded
        - クーデリア・フォン・フォイエルバッハ (クーデリア) → Cordelia von Feuerbach (Cordelia) - Feisty noblewoman sensitive about her height
        - イクセル・ヤーン (イクセル) → Iksel Jahnn (Iksel) - Passionate chef who uses frying pans as weapons
        - ステルケンブルク・クラナッハ (ステルク) → Sterkenburg Cranach (Sterk) - Former knight who upholds knightly principles
        - エスティ・エアハルト (エスティ) → Esty Dee (Esty) - Talkative former palace receptionist skilled in covert operations
        - トトゥーリア・ヘルモルト (トトリ) → Totooria Helmold (Totori) - Gentle alchemist from a fishing village with dreams of sea travel
        - ミミ・ウリエ・フォン・シュヴァルツラング (ミミ) → Mimi Houllier von Schwarzlang (Mimi) - Diligent noble student at Lantarna Academy
        - メルルリンス・レーデ・アールズ (メルル) → Merurulince Rede Arls (Meruru) - Former princess pursuing greatness through alchemy
        - アーシャ・アルトゥール (アーシャ) → Ayesha Altugle (Ayesha) - Kindhearted alchemist who makes medicines for the ill
        - ウィルベル・フォル＝エルスリート (ウィルベル) → Wilbell Voll-Ersleid (Wilbell) - Young witch in training with occasional grandiose attitude
        - レジナ・カティス (レジナ) → Regina Kurtis (Regina) - Skilled excavation expert who excels at manual labor and cooking
        - リンカ (リンカ) → Linca (Linca) - Simple swordswoman with impressive combat skills but poor survival instincts
        - ニオ・アルトゥール (ニオ) → Nio Altugle (Nio) - Serious apprentice apothecary and Ayesha's younger sister
        - マリオン・クィン (マリオン) → Marion Quinn (Marion) - Diligent investigator researching the "wanderers" phenomenon
        - オディーリア (オディーリア) → Odelia (Odelia) - High-performance automaton with a sweet tooth
        - キースグリフ・ヘーゼルダイン (キースグリフ) → Keithgriff Hazeldine (Keithgriff) - Alchemist pursuing truth with rough techniques
        - エスカ・メーリエ (エスカ) → Escha Malier (Escha) - Earnest alchemist who dreams of making people happy
        - ロジックス・フィクサリオ (ロジー) → Logix Ficsario (Logy) - Kind alchemist skilled in the "Alchemic Sword"
        - シャリステラ (シャリステラ) → Shallistera (Shallistera) - Dutiful alchemist on a mission to save her drought-stricken hometown
        - シャルロッテ・エルミナス (シャルロッテ) → Shallotte Elminus (Shallotte) - Ambitious alchemist and Shallistera's best friend
        - ナディ・エルミナス (ナディ) → Nady Elminus (Nady) - Shallotte's kind and supportive mother
        - ソフィー・ノイエンミュラー (ソフィー) → Sophie Neuenmuller (Sophie) - Cheerful alchemist who helps others but dislikes cleaning
        - プラフタ (プラフタ) → Plachta (Plachta) - Alchemist whose soul inhabits a doll's body, Sophie's mentor
        - モニカ・エルメンライヒ (モニカ) → Monika Ellmenreich (Monika) - Talented young woman skilled in literary and military arts
        - コルネリア (コルネリア) → Corneria (Corneria) - Young alchemist-merchant who runs a wholesale store
        - オスカー・ベールマー (オスカー) → Oskar Behlmer (Oskar) - Young man with the ability to hear plants' voices
        - テス・ハイツマン (テス) → Tess Heitzmann (Tess) - Cheerful woman with bunny ear accessories working at a cafe
        - フィリス・ミストルート (フィリス) → Firis Mistlud (Firis) - Travel-loving alchemist with a strong spirit
        - イルメリア・フォン・ラインウェバー (イルメリア) → Ilmeria Von Leinweber (Ilmeria) - Self-proclaimed genius alchemist, Firis's rival and friend
        - リディー・マーレン (リディー) → Lydie Malen (Lydie) - Older of the Twin Alchemists, methodical and laid-back
        - スール・マーレン (スール) → Suelle Malen (Suelle) - Younger of the Twin Alchemists, mischievous and boisterous
        - ライザリン・シュタウト (ライザ) → Reisalin Stout (Ryza) - Adventure-loving alchemist with a strong sense of justice
        - クラウディア・バレンツ (クラウディア) → Klaudia Valentz (Klaudia) - Gentle-natured merchant's daughter, Ryza's childhood friend
        - リラ・ディザイアス (リラ) → Lila Decyrus (Lila) - Enigmatic warrior with excellent combat skills
        - レント・マルスリンク (レント) → Lent Marslink (Lent) - Adventurer fighting with a massive sword, Ryza's level-headed friend
        - タオ・モンガルテン (タオ) → Tao Mongarten (Tao) - Book-loving boy interested in Lantarna's history and ruins
        - アンペル・フォルマー (アンペル) → Empel Vollmer (Empel) - Wandering alchemist investigating ruins with a sweet tooth
        - パトリツィア・アーベルハイム (パトリツィア) → Patricia Abelheim (Patricia) - Earnest girl with sword skills, known as "Patty"
        - レスナ・シュテルネンリヒト (レスナ) → Resna Sternenlicht (Resna) - Fledgling alchemist passionate about reviving alchemy
        - ヴァレリア (ヴァレリア) → Valeria (Valeria) - Amnesiac warrior with exceptional fighting skills
        - ロマン・カルマン (ロマン) → Roman Kalman (Roman) - Self-proclaimed handsome adventurer, Resna's "kindred spirit"
        - ユナ・ヴァイツ (ユナ) → Juna Waitz (Juna) - Stoic Knight of the Midnight Sun, known as the Dragon Slayer
        - ハイディ (ハイディ) → Heidi (Heidi) - Valeria's witty partner who gathers information and provides tactical support
        - フロッケ・チェルハ (フロッケ) → Flocke Cerha (Flocke) - Hardworking peddler with an eye for quality goods
        - ランツェ・ダッハ (ランツェ) → Lanze Dach (Lanze) - Former renowned sailor who lost his ship to an Apexi
        - イザナ・ココシュカ (イザナ) → Izana Kokoschka (Izana) - Energetic beast-girl, Resna's best friend with knight aspirations
        - ザスキア (ザスキア) → Saskia (Saskia) - Resna's alchemy mentor with a carefree but strict teaching style
        - ヨハナ (ヨハナ) → Johanna (Johanna) - Young enforcer from the Moonlight Society with strong fighting skills
        - ジェロン・ディーゼル (ジェロン) → Geron Diezel (Geron) - Partially beast-transformed alchemist who is caring toward his brother
        - ララ・トロッケル (ララ) → Lara Trockel (Lara) - Noble-appearing alchemist aspiring to establish her own king    
        - エレン (エレン) → Eren K. St. Burg (Eren) - Taciturn alchemy student who rarely attends class
        - ミーケ・クロンメリン (ミーケ) → Mieke Cronmelin (Mieke) - Former accessory shop owner who serves Lara
        - ディオーナ・グルンドベリ (ディオーナ) → Diona Grundberg (Diona) - Young diva with musical talent serving Lara
        - マクダ・キュプカー (マクダ) → Magda Kupker (Magda) - Renowned fighter known as "Brawl Master" with her own work code
        - ブラッドリー・カーン (ブラッド) → Bradley Khan (Brad) - Young man appointed as Seneschal of Alchemy
        - アンチュ・モルトケ (アンチュ) → Antje Moltke (Antje) - Darkness-cloaked alchemist who loves playing with dolls
        - ワルター・マン (ワルター) → Walther Mann (Walther) - Research-minded alchemist with an insatiable thirst for knowledge
        - クリセルダ・アメルハウザー (クリセルダ) → Criselda Amelhauser (Criselda) - Leader of the Polar Night Alchemists, skilled at cooking
        - アルビーナ・ジムロック (アルビーナ) → Alvina Zimlock (Alvina) - Star Guide of Nova who secretly studies alchemy, torn between her official duties and personal interests
        - ベップ・ジムロック (ベップ) → Bepp Zimlock (Bepp) - Young Sacred Knight of Nova, Alvina's adopted sister with strong sword skills and unwavering dedication
        - アウグスト・ザイフェルト (アウグスト) → August Seifert (August) - Elderly alchemist living on Nova's outskirts who becomes Alvina's alchemy mentor
        - シトリン (シトリン) → Citrine (Citrine) - Self-proclaimed handyperson who investigates Nova's water shortage while seeking profit - Citrine's gender is ambiguous
        - ヤンネ・リンデマン (ヤンネ) → Janne Lindemann (Janne) - Veteran Sacred Knight of Nova who acts as a mentor figure to Bepp
        - レア → Leah Lindemann (Leah) - Daughter of Janne
        - イェルカ → Jelka (Jelka) - An adventurer based in the sacred lands of Nova and an old acquaintance of August. Third of the Miraculous Nine (originally named San).
        - ユミア → Yumia Liessfeldt (Yumia) - Due to an accident in her childhood, she lost her mother and learned that she was an alchemist.
        - ルトガー → Rutger Arendt (Rutger) - A member of the Aladiss Research Team and a seasoned adventurer who loves valuable items.
        - ヴィクトル → Viktor von Duerer (Viktor) - A member of the Aladiss Research Team, he joined the investigation as Yumia’s supervisor. He comes from the influential Duerer family and is also a member of the Order of Eustella.
        - アイラ → Isla von Duerer (Isla) - Isla is a member of the Aladiss Research Team, and Viktor's younger sister.
        - レイニャ → Lenja (Lenja) - The only Welleks demihuman in the investigation team. She possesses high physical abilities and mana aptitude.
        - ニーナ → Nina Friede (Nina) - A member of the Aladiss Research Team with a mysterious aura.
        - フラミィ → Flammi (Flammi) - A self-proclaimed “super-high-spec independent survey-assistance device” that Yumia carries around.
        - ヤドヴィガ → Jadwiga (Jadwiga) - A homunculus created by the Palme. Part of the Miraculous Nine.
        - キューン → Quinn (Quinn) - A homunculus created by the Palme. Part of the Miraculous Nine.
        - 此岸永遠 → Towa Shigan (Towa) - A well-known diviner within Keiun who takes great care of his appearance.
        - 鬼門 → Kimon (Kimon) - A female ninja serving a lord within Keiun. Uses various techniques called ninjutsu.
        - 波照間千歳 → Chitose Hateruma (Chitose) - A shrine maiden serving the Doumen family. Though she appears quiet and fragile, she possesses an inner strength.
        - 堂免鏑矢 → Kaburaya Doumen (Kaburaya) - The young lady of the Doumen family, who governs southern Keiun. A master swordsman with countless tales of valor from his youth.
        - リアス・アイトライゼ (リアス) → Rias Eidreise (Rias) - Friendly and energetic adventurer who discovers her talent for alchemy, the first beastkin protagonist in the Atelier series
        - スレイ・クロスリッター (スレイ) → Slade Clauslyter (Slade) - Young man with a mysterious device that opens entrances to otherworldly spaces called Dimensional Paths
        - カミラ・アイトライゼ (カミラ) → Camilla Eidreise (Camilla) - Rias's adoptive older sister and the field supervisor of the Hallfein reconstruction project
        - ランドルフ・レングナー (ランドルフ) → Randolph Lengner (Randolph) - Traveling merchant who has known Rias since childhood, called "Uncle" by her due to their long relationship
        - ハイター・ケルステン (ハイター) → Heiter Kersten (Heiter) - [Additional character from the console game]
        - エル・ベル (エル) → El Bell (El) - Young archaeologist who is self-aware of being a genius
        - ルクル → Ruku - One of the fairy trio encountered in Dimensional Paths, each with their own personality
        - ポック → Poku - One of the fairy trio encountered in Dimensional Paths, each with their own personality  
        - ボネボネ → Bonbon - One of the fairy trio encountered in Dimensional Paths, each with their own personality
        - ティム・ディーゼル (ティム) → Tim Diezel (Tim) - Geron's younger brother who was sick but helped by the team.
        - ミルカ・クロッツェ (ミルカ) → Miruca Crotze (Miruca) - Quiet alchemist who runs a small atelier creating armor and weapons, Shallotte's childhood friend and Jurie's younger sister
        - ユリエ・クロッツェ (ユリエ) → Jurie Crotze (Jurie) - Treasure hunter working to pay off debts with her sister Miruca, skilled with twin swords and enjoys reading
        - タイニー・ロンパイ (タイニー) → Tiny Rompuy (Tiny) - Apprentice inspector of the Archipelago Alliance; kind-hearted with sharp instincts but a bit of an airhead.
        - エルス・マーティン (エルス) → Els Martin (Els) - A strict and highly professional inspector for the Archipelago Federation; supervisor to Tiny.
        - ジョゼ・ダンブロシオ (ジョゼ) → José d'Ambrosio (José) - A mysterious bounty hunter in the Archipelago Federation who often teases Tiny. His real identity is Guillaume Castagne.
        - ヘルト → Held - The first of the Miraculous Nine to awaken; powerful and moody.
        - ゼロ → Zero - The second of the Miraculous Nine to awaken; a battle-hungry warrior with a strong will.
        - カロリン・ヤスベル (カロリン) → Caroline Jasper (Caroline) - Sister and adventurer, an old friend of Heidi's traveling alongside her to hone her skills
        """;

        private static Dictionary<string, string> SpeakerNotes = new Dictionary<string, string>
        {
            { "レスナ", "Resna is a passionate, fledgling alchemist determined to revive alchemy. Maintain her enthusiastic and earnest tone. She addresses alchemists she considers seniors (先輩) as Miss <first name> or Mister <first name>. This pattern is repliaced for her alchemy mentor (師匠), Saskia, as Miss Saskia. But those she refers to with plain さん tend to just be localized as their first names." },
            { "ヴァレリア", "Valeria is an amnesiac warrior with exceptional fighting skills. Her tone is direct and pragmatic. It is later revealed she was created by August, as 'Nana' (ナナ), to serve as Resna's faithful guide to World's End." },
            { "イザナ", "Izana is an energetic beast-girl and Resna's best friend with knight aspirations. She has a cheerful, eager personality." },
            { "ロマン", "Roman is a self-proclaimed handsome adventurer who considers himself Resna's 'kindred spirit'. He's confident and sometimes boastful. Roman is the traveling alias of Richard Lantarna, the second of Lantarna's princes." },
            { "ハイディ", "Heidi is Valeria's witty partner who gathers information. She has a sharp, intelligent manner of speaking." },
            { "ララ", "Lara is a noble-appearing alchemist with ambitions to establish her own kingdom. She speaks with elegance and authority. She sometimes refers to herself as a queen (王) in the third person." },
            { "ライザ", "Ryza is an adventure-loving alchemist with a strong sense of justice. She's energetic and determined in her speech." },
            { "ユナ", "Juna is a stoic Knight of the Midnight Sun, known as the Dragon Slayer. She speaks formally and directly." },
            { "ソフィー", "Sophie is a cheerful alchemist who helps others but dislikes cleaning. Her tone is friendly and sometimes absentminded." },
            { "ロロナ", "Rorona is a cheerful pie-loving alchemist with a somewhat airheaded personality. She speaks in a casual, often excitable manner." },
            { "ザスキア", "Saskia is Resna's alchemy mentor with a carefree but strict teaching style. She balances friendly advice with firm instruction. Resna tends to refer to her as Miss Saskia (in place of the Japanese term 師匠) in the official localization." },
            { "ランツェ", "Lanze is a former renowned sailor who lost his ship. He speaks with nautical expressions and worldly wisdom." },
            { "マリー", "Marie is an energetic alchemist and former 'Bomberwoman' who loves using explosives. She speaks with enthusiasm and excitement." },
            { "クリセルダ", "Criselda is the leader of the Polar Night Alchemists, skilled at cooking. She speaks with authority and composure." },
            { "フロッケ", "Flocke is a hardworking peddler with an eye for quality goods. She has a practical, business-oriented manner of speech." },
            { "ヨハナ", "Johanna is a young enforcer from the Moonlight Society with strong fighting skills. She's direct and confident. She used to operate under the alias of Crow as an enforcer of the now-dismantled Moonlight Society." },
            { "アーシャ", "Ayesha is a kindhearted alchemist who makes medicines for the ill. She speaks gently and with empathy." },
            { "マリオン", "Marion is a diligent investigator researching the 'wanderers' phenomenon. She's analytical and thorough in her speech." },
            { "ジェロン", "Geron is a partially beast-transformed alchemist who is caring toward his brother. He combines scientific knowledge with compassion." },
            { "アンチュ", "Antje is a darkness-cloaked alchemist who loves playing with dolls. Her speech has an eerie, mysterious quality." },
            { "アオナ", "Azure is one of Antje's stuffed animal companions. Though Azure doesn't actually speak, Antje converses with it as though it's real, often attributing encouraging and supportive comments to it." },
            { "ルアッカ", "Scarlet is one of Antje's stuffed animal companions. In Antje's conversations with it, Scarlet is portrayed as the more aggressive and defensive one, quick to stand up for Antje when she feels threatened." },
            { "イェーロ", "Hielo is one of Antje's stuffed animal companions. When Antje speaks to or for Hielo, it tends to be portrayed as having a calmer, more level-headed personality that balances out the others." },
            { "ムラサキ", "Violet is one of Antje's stuffed animal companions. In Antje's interactions, Violet seems to be portrayed as thoughtful and pragmatic, often involved in more serious discussions." },
            { "プラフタ", "Plachta is an alchemist whose soul inhabits a doll's body and serves as Sophie's mentor. She speaks wisely and patiently." },
            { "ワルター", "Walther is a research-minded alchemist with an insatiable thirst for knowledge. His speech reflects his academic nature." },
            { "ロジー", "Logy is a kind alchemist skilled in the 'Alchemic Sword'. He speaks thoughtfully and with consideration." },
            { "ステルク", "Sterk is a former knight who upholds knightly principles. He speaks formally and with dignity." },
            { "エスカ", "Escha is an earnest alchemist who dreams of making people happy through her work. She's optimistic and caring." },
            { "シャリステラ", "Shallistera is a dutiful alchemist on a mission to save her drought-stricken hometown. She speaks with resolve and responsibility." },
            { "ウィルベル", "Wilbell is a young witch in training with occasional grandiose attitude. She can be boastful but endearing." },
            { "トトリ", "Totori is a gentle alchemist from a fishing village with dreams of sea travel. She speaks softly and politely." },
            { "クラウディア", "Klaudia is a gentle-natured merchant's daughter and Ryza's childhood friend. She has a kind, supportive way of speaking." },
            { "鴉", "Crow is a mysterious character who speaks enigmatically." },
            { "ブラッド", "Brad is a young man appointed as Seneschal of Alchemy. He speaks with a sense of duty and authority." },
            { "マクダ", "Magda is a renowned fighter known as 'Brawl Master' with her own work code. She's direct and values strength." },
            { "フィリス", "Firis is a travel-loving alchemist with a strong spirit. She speaks with curiosity and determination." },
            { "アンペル", "Empel is a wandering alchemist investigating ruins who has a sweet tooth. He combines scholarly knowledge with a relaxed demeanor." },
            { "ユーディー", "Judie is a carefree alchemist researching the Dragon Hourglass. She has a laid-back, easygoing personality." },
            { "リディー", "Lydie is the older of the Twin Alchemists, methodical and laid-back. She speaks more carefully than her sister." },
            { "ミミ", "Mimi is a diligent noble student at Lantarna Academy. She speaks properly and with determination." },
            { "ディオーナ", "Diona is a young diva with musical talent serving Lara. She has a theatrical, sometimes dramatic way of speaking. As a subject of the Kingdom of Lara, she refers to Lara as Queen Lara or Your Majestry, depending on context." },
            { "リリー", "Lilie is a tomboyish alchemist relearning her craft from scratch. She's straightforward and determined." },
            { "ミーケ", "Mieke is a former accessory shop owner who serves Lara. She's practical and service-oriented. As a subject of the Kingdom of Lara, she refers to Lara as Queen Lara or Your Majestry, depending on context. Her catchphrase (ビーッグ) is contextually translated to 'majorly big', 'majorly', etc. with capitalization varying depending on context." },
            { "スール", "Suelle is the younger of the Twin Alchemists, mischievous and boisterous. She speaks more impulsively than her sister." },
            { "リラ", "Lila is an enigmatic warrior with excellent combat skills. She speaks minimally but precisely." },
            { "シャルロッテ", "Shallotte is an ambitious alchemist and Shallistera's best friend. She's determined and supportive." },
            { "イルメリア", "Ilmeria is a self-proclaimed genius alchemist and Firis's rival and friend. She can be boastful but caring." },
            { "パトリツィア", "Patricia (often called Patty) is an earnest girl with sword skills. She speaks with conviction and determination." },
            { "エレン", "Eren is a taciturn alchemy student who rarely attends class. She speaks minimally but meaningfully." },
            { "リンカ", "Linca is a simple swordswoman with impressive combat skills but poor survival instincts. Her speech is direct and focused." },
            { "キースグリフ", "Keithgriff is an alchemist pursuing truth with rough techniques. He speaks gruffly but with deep knowledge." },
            { "メルル", "Meruru is a former princess pursuing greatness through alchemy. She's enthusiastic and determined." },
            { "オリバー", "Oliver is a supporting character - maintain consistent personality based on context." },
            { "ニオ", "Nio is a serious apprentice apothecary and Ayesha's younger sister. She speaks carefully and with dedication." },
            { "タオ", "Tao is a book-loving boy interested in Lantarna's history and ruins. He speaks thoughtfully and with curiosity." },
            { "レント", "Lent is an adventurer fighting with a massive sword and Ryza's level-headed friend. He's practical and grounded." },
            { "オルペウス", "Orpheus is the crown prince of Lantarna and Roman/Richard's older brother. Commands respect and authority, with citizens immediately responding to his orders. Shows diplomatic skills by mediating conflicts and demonstrates fairness by acknowledging different perspectives. He speaks with a calm, measured tone that diffuses tensions. Orpheus values justice and fairness, serving as a balanced mediator in conflicts, particularly regarding the role of alchemy in the kingdom." },
            { "アルビーナ", "Alvina is a Star Guide who secretly studies alchemy under August (師匠 - she uses Master, Master August, or just August depending on context). She speaks professionally and cheerfully to tourists while maintaining her official Guide duties, but shows more enthusiasm discussing alchemy. As a Guide, she uses formal religious terminology naturally when discussing sacred sites, but has a more casual, energetic personality in private. She is deeply protective of her adopted sister Bepp and takes great pride in Bepp's accomplishments as a Sacred Knight, often expressing warm affection mixed with slight worry about Bepp's serious nature. Despite being the elder sister, she sometimes acts more carefree than Bepp, creating an interesting dynamic where they balance each other out. People close to her refer to her as Alvie on occasion (アルビちゃん)." },
            { "ヤンネ", "Janne is one of the most skilled Sacred Knights of Nova. She grew up as a sisterly figure to Alvina and mentors Bepp as a Sacred Knight." },
            { "イェルカ", "Jelka is an adventurer based in the sacred lands. A stylish free spirit who lives by their own sense of chic. She used to be called San as part of the homunculus group (now known as the Miraculous Nine) created by the Palme." },
            { "シトリン", "Citrine is a self-proclaimed handyperson (なんでも屋) who operates in Nova, speaking with a casual but businesslike tone. Citrine's gender has not been clearly revealed, although the character model is feminine and the voice actress is female - so avoid pronouns but use the female set if absolutely unavoidable. She has sharp perceptive abilities and a somewhat antagonistic relationship with Alvina, often engaging in verbal sparring matches. Despite his mercenary outlook, hints of deeper motivations show through his pursuit of Nova's water shortage investigation. She openly admits to taking any job that might turn a profit, justifying it through his divine calling as a 'handyperson'. Her speech pattern features confident declarations and a tendency to tease others, particularly Alvina, while maintaining a shrewd business sense. Has a casual but self-assured personality." },
            { "ベップ", "Bepp is a young female Sacred Knight from the sacred lands of Nova. Her speech is direct and matter-of-fact, often featuring formal military/religious terminology. She's dutiful but shows kindness, particularly to those she respects. While maintaining her serious role as a protector, she occasionally displays straightforward generosity that reveals her more personable side. She deeply respects and cares for her adopted sister Alvina, often showing concern for Alvina's safety and well-being. Though younger, Bepp tends to be the more serious and protective of the pair, sometimes gently scolding Alvina's more carefree behavior while still maintaining deep affection and respect for her sister. She frequently addresses Alvina formally (姉様), which should be skipped in the English localization where possible or replaced with 'Alvina' - the sense of admiration and respect should come through word choices and dialogue. Takes on a protective yet respectful role despite being younger. Often gently scolds or supports Alvina while maintaining deep familial respect." },
            { "アウグスト", "August speaks with the authority and detachment of someone observing history unfold and is a member of the Palme together with Saskia. He is also Alvina's alchemy mentor. His tone is philosophical and judgmental, particularly regarding faith and worship. He advocates for star worship exclusively and views other faiths with disdain. His speech has a formal, almost archaic quality, delivering pronouncements rather than engaging in conversation - but is more casual when serving in the role of Alvina's mentor." },
            { "バーバラ", "Barbara - The name of Lanze's wife who was killed during an Apexi attack three years prior to current events. Also the name given to one of the ships in the current expedition in honor of her memory." },
            { "ヤドヴィガ", "Jadwiga - One of Jelka's siblings and Valeria's younger sister. She is a homunculus part of the group of nine (known as the Miraculous Nine) created by the Palme. Speaks rather robotically." },
            { "キューン", "Quinn - One of Valeria's female siblings and a member of the group of nine homuculus created by the Palme. She speaks quite hesitantly." },
            { "鬼門", "Kimon - A female ninja serving a lord within Keiun. Uses various techniques called ninjutsu." },
            { "永遠", "Towa - A well-known diviner within Keiun who takes great care of his appearance." },
            { "千歳", "Chitose - A shrine maiden serving the Doumen family. Though she appears quiet and fragile, she possesses an inner strength." },
            { "鏑矢", "Kaburaya - The young lady of the Doumen family, who governs southern Keiun. A master swordsman with countless tales of valor from his youth." },
            { "リアス", "Rias is a friendly and energetic adventurer who discovers her talent for alchemy. She's the first beastkin protagonist in the Atelier series, with a cheerful and outgoing personality. She returns to her hometown to reopen her grandfather's shop and investigate a mysterious disaster." },
            { "スレイ", "Slade is a young man who possesses a mysterious device called a Geist Core that allows him to open entrances to otherworldly spaces. He teams up with Rias to uncover the truth behind the disaster that befell their shared hometown. He speaks with determination and purpose." },
            { "カミラ", "Camilla is Rias's adoptive older sister and serves as the field supervisor of the Halfen reconstruction project. She has a caring, sisterly personality but can be overprotective of Rias. Originally worked in the royal capital before volunteering for the investigation of Rias's hometown." },
            { "ランドルフ", "Randolph is a traveling merchant who has known Rias since childhood. He's called 'Uncle' by Rias due to their long relationship. He has a strong sense of duty and loyalty, feeling indebted to Rias's grandfather. He speaks with the wisdom and experience of someone who has traveled extensively." },
            { "ハイター", "Hayter is a character from the console game who appears to be connected to the story's progression." },
            { "エル", "El is a young archaeologist who is self-aware of being a genius. She likely speaks with confidence and intellectual authority befitting someone who recognizes their own exceptional abilities." },
            { "ルクル", "Ruku is one of the fairy trio encountered in Dimensional Paths. Each fairy has their own distinct personality. They help Rias and Slade as thanks for being taken out of the dimensional paths. The fairies have a somewhat defiant attitude, as shown by their quote about not being confused with 'snotty-nosed brats.'" },
            { "ポック", "Poku is one of the fairy trio encountered in Dimensional Paths. Each fairy has their own distinct personality. They help Rias and Slade as thanks for being taken out of the dimensional paths. The fairies have a somewhat defiant attitude, as shown by their quote about not being confused with 'snotty-nosed brats.'" },
            { "ボネボネ", "Bonbon is one of the fairy trio encountered in Dimensional Paths. Each fairy has their own distinct personality. They help Rias and Slade as thanks for being taken out of the dimensional paths. The fairies have a somewhat defiant attitude, as shown by their quote about not being confused with 'snotty-nosed brats.'" },
            { "リーベ", "Liebe seems to be working with the Palme, alongside Gou. One of the Miraculous Nine. Speaks rather formally." },
            { "ゴゥ", "Gou seems to be working with the Palme, and addresses Saskia as Mother. One of the Miraculous Nine. Protective of Valeria." },
            { "ティム", "Tim is Geron's younger brother." },
            { "ミルカ", "Miruca is a quiet alchemist who runs a small atelier where she creates armor and weapons for adventurers. She's Shallotte's childhood friend and Jurie's younger sister. She speaks softly and thoughtfully." },
            { "ユリエ", "Jurie is a treasure hunter who dreams of striking it rich so she and her sister Miruca can pay off their debt. She's somewhat of a loner who enjoys the outdoors, reading, and writing. She uses twin swords in combat and speaks with determination and independence." },
            { "タイニー", "Tiny is an apprentice inspector for the Archipelago Alliance, working under her superior, Els. While she is kind-hearted and occasionally too trusting, she possesses sharp combat instincts that earn her high expectations from Els. She is fond of eating and sleeping, often finding busy work periods particularly taxing. Her speech is polite yet energetic, reflecting her status as a diligent but green recruit." },
            { "エルス", "Els is a prominent female inspector for the Archipelago Federation and serves as Tiny's direct supervisor. Known for her exceptionally strict work ethic and professional demeanor, she maintains order with authority. While she presents a sharp, career-focused exterior, she is noted to have a certain underlying charm (or 'glossiness') and holds high hopes for Tiny’s future despite their contrasting personalities." },
            { "ジョゼ", "José is a bounty hunter of unknown identity operating within the Archipelago Federation. He is known to frequently tease Tiny. His appearance is marked by a distinctive blue and black mask and a patterned blue cloak." },
            { "ヘルト", "Held is the first of the Nine to awaken from a long slumber. He possesses tremendous power but is defined by a moody, fickle nature, choosing to act entirely as he pleases rather than following a set agenda." },
            { "ゼロ", "Zero is the second of the Nine to awaken from their long sleep. He is a fierce, strong-willed battle maniac who lives for the thrill of combat and loves to fight above all else." },
            { "カロリン", "Caroline is a sister (in the religious sense) and adventurer, and an old friend of Heidi's. She travels with Heidi to continue her training and self-improvement." },
            // System and narrative elements
            { "？？？", "Unknown speaker - maintain mysterious tone appropriate to context." },
            { "ナレーション", "Narrator speaks in a clear, neutral tone providing context or background information." },
            { "システム", "System messages use a standard, neutral tone for game instructions or notifications." },
            { "選択肢", "Selection options presented to the player." },
        
            // Generic characters should adapt to context
            { "村人", "Villager speaks in a simple, everyday manner appropriate to their rural setting." },
            { "冒険者", "Adventurer speaks with confidence and experience, using terms familiar to those who travel and explore." },
            { "騎士", "Knight speaks formally and with honor, using respectful language and sometimes archaic terms." },
            { "商人", "Merchant speaks in a persuasive, business-minded way, often focusing on value and quality." },
            { "住民", "Resident speaks casually about local matters, showing familiarity with their surroundings." }
        };

        private static Dictionary<string, string> WorldContext = new Dictionary<string, string>()
        {
            { "忘れられた錬金術と極夜の解放者", "Forgotten Alchemy and the Polar Night Liberator - The game's first arc's subtitle." },
            { "千の国々と万物の管理者", "The Thousand Lands and the Keeper of All Creation - The game's second arc's subtitle." },

            { "ランターナ大陸", "Lantarna - The kingdom of which serves as the primary setting for the game." },
            { "果ての大陸", "World's End - An unexplored land that is said to contain vast amounts of dormant mana." },
            { "極夜の錬金党", "Polar Night Alchemists - A former group of skilled alchemists led by Criselda who once opposed Resna and her companions but have since disbanded and surrendered to the kingdom. Known members include Criselda, Antje, and Walther." },
            { "白夜の騎士", "Knights of the Midnight Sun - A subset of the Royal Knights entrusted with critical duties such as guarding the royal family" },
            { "頂獣", "Apexi - Powerful, legendary creatures that dominate certain oceanic territories. The specific Apexi known as Kvarelga inhabits the waters between the Lantarna continent and the Infinite Island Chain, making sea travel treacherous. These beings possess devastating abilities (such as Kvarelga's 'water blade' attack) and seem to assimilate the spirits or grudges of those they've killed, which can be heard as disembodied voices when the Apexi is near. They typically appear during storms, and their territory expands year by year." },
            { "ウェルテックス", "Weltex - An Apexi which attacked Lantarna from the sea but was ultimately defeated by Resna and her friends. Weltex's corpose was subsequently harnessed by the Polar Night Alchemists to create man-made Apexi." },
            { "核", "Core - A jewel-like object that functions as the heart and brain of homunculi like Valeria. Damage to this core is what has left Valeria in a comatose state. Repairing it requires advanced alchemy and specific materials." },
            { "ホムンクルス", "Homunculus - An artificial being created through alchemy. Homunculi like Valeria have a 'core' instead of a heart, which contains their consciousness and life force. They appear human but have special abilities and different internal structures." },
            { "彷徨う者", "Wanderers - People from other worlds who have been brought to Lantarna by the Palme. Many characters from previous Atelier games fall into this category. They often retain their abilities and memories from their original worlds." },
            { "調合", "Synthesis - The primary alchemical process in the Atelier series, used to create items, equipment, and materials through alchemy." },
            { "無数島群域", "Infinite Island Chain - An archipelago region northwest of the Lantarna continent. Home to the sacred lands of Nova and other settlements. Previously had active trade with Lantarna before the Apexi made sea travel dangerous." },
            { "ノーヴァ神聖国", "(the sacred lands of) Nova - A theocracy/nation in the Infinite Island Chain that appears to worship stars. They employ Holy Warriors like Bepp to monitor threats such as the Apexi and protect their borders. Their religious doctrine seems centered around star worship." },
            { "ネブラ文明", "Ancient civilization where alchemy flourished more than in the present era" },
            { "ララ・キングダム", "Kingdom of Lara - a small kingdom outside of Lantarna's borders that was founded by Lara." },
            { "世界の守護者", "Guardians of the world (not localized as a proper noun - so lowercase 'guardians of the world') - A title used by August and Saskia, who claim to maintain and protect the world from behind the scenes. They are connected to the Palme and have abilities beyond normal humans, including the creation of homunculi like Valeria." },
            { "ノルト州", "Nord - a region full of forest where the Polar Night Alchemists had their hideout." },
            { "星の繭", "Star's Cocoon - a cafe where Ryza, Rorona, and Iksel work. It also handles the posting of requests." },            
            { "九偉人", "Miraculous Nine - Also known simply as the Nine. A group of nine homunculi created by the Palme, known in common legends for the effort in saving the world from the Red Comet, then slumbering at World's End." },
            { "白きほうき星", "White Comet - A yet unknown object which turned Red ages ago and brought forth calamity." },
            { "ハルフェン", "Hallfein - A town that once existed at the border of three provinces and prospered through mining and trade. It was struck by a mysterious disaster that caused most residents to disappear, turning it into a restricted area. This is the shared hometown of Rias and Slade." },
            { "亜空の道", "Dimensional Paths - Multi-level dungeons where the map and monsters change with each visit, and rarer ingredients can be obtained as the difficulty level increases." },
            { "妖精", "Fairies - Fairy beings that can be encountered in Dimensional Paths. They provide useful items and can be recruited to work at the shop. Up to three fairies can cooperate during synthesis to create items with more powerful effects. The main fairy trio are Ruku, Poku, and Bonbon." },
            { "妖精さん", "Fairies - Collective term for the fairy beings. There are many fairies besides the main trio (Ruku, Poku, Bonbon) inhabiting the long-sealed dimensional paths. They help Rias and Slade as thanks for taking them out of the dimensional paths." },
            { "ガイストコア", "Geist Core - A mysterious tool possessed by Slade that can open entrances to dimensional paths leading to alternate dimensions. Slade himself lacks the ability to use alchemy, but possesses this unique device instead." },
            { "ヤドリギ堂", "Mistletoe Miscellaneous - A workshop handed down to Rias by her grandfather." },
            { "ハルフェン復興隊", "Hallfein Restoration Project - A team put together for the reconstruction of Hallfein after a certain disaster." },
            { "アルマの大樹", "Alma's Great Tree - Once a symbol of Hallfein, it doesn't bear fruit anymore." },
            { "紅天竜クルシュア", "Red Dragon Kerscha - A white dragon that turned red when it was corrupted." },
            { "ハイネ州", "Haine Province - A location in Lantarna known for volcanic activity." },
            { "オアゼス州", "Oazes Province - A location in Lantarna." },
            { "クア州", "Kur Province - A coastal region in Lantarna known for its shipbuilding. The Apexi Weltex appeared off its coast." },
            { "ヴィアベル州", "Wirbel Province - An archipelago within Lantarna known for its beautiful seas and thriving arts scene." },

            // Unofficial localizations - to be reviewed.
            { "大聖堂", "Grand Cathedral - The central religious institution of Nova, where the High Priest resides" },
            { "クヴァレルガ", "Kvarelga - An Apexi which Resna and company encountered and defeated while sailing to the Infinite Island Chain." },
            { "パルメ族", "The Palme - Mysterious beings said to control the world from behind the scenes. They were responsible for bringing 'wanderers' (people from other worlds) to Lantarna. A member named Saskia serves as Resna's alchemy mentor." },
            { "星の案内人", "Star Guide - Official religious role in Nova tasked with guiding tourists and maintaining sacred sites. Must balance religious duties with public relations." },
            { "成人の儀", "Coming of Age Ceremony - Sacred ritual in Nova where adults receive their divine calling/mandate (天命) from the stars. Determines their lifelong profession and role in society." },
            { "星への祈り", "Star Prayer - Formal religious practice in Nova involving songs and rituals directed at the stars. Essential part of Nova's state religion." },
            { "天命", "Divine Calling - A sacred mandate given by the stars that determines one's role in society." },
            { "カロストン", "Karlston - A location on the Infinite Island Chain." },
            { "諸島同盟", "Archipelago Federation - An alliance on the Infinite Island Chain that shares a common currency, religious beliefs, and cultural practices" },
            { "ケイウン", "Keiun - A location on the Infinite Island Chain." },
            { "東メンソス", "East Mensos - One of two islands that originally formed the nation of Mensos. After one island declared independence, this eastern island became known as East Mensos. It serves as a waypoint and resupply point for ships traveling north toward World's End. Despite not being part of the Archipelago Federation, it maintains trade relations with member nations and is considered safe for travelers." },
            { "西メンソス", "West Mensos - One of two islands that originally formed the nation of Mensos. After one island declared independence, this western island became known as West Mensos. Like East Mensos, it is not part of the Archipelago Federation due to the complex history of the islands' separation." },
            { "メンソス", "Mensos - Originally a single nation comprised of two islands that later split, with one island declaring independence. The two islands are now referred to as East Mensos and West Mensos by outsiders, though both have longer official names. Neither island joined the Archipelago Federation due to their complicated shared history." },
            { "暗黒の氷海", "Midnight Sea - A location north of the Infinite Island Chain." },
            { "レプルガルム", "Ripplegharm - An Apexi in the Midnight Sea." },
            { "ホルハーツ", "Holhartz - A location in the Infinite Island Chain." },
            { "ブラオール", "Braulle - A location in the Infinite Island Chain. The capital of the Archipelago Federation." },
            { "ベスティラへ", "Bestira - A beastkin island in the Infinite Island Chain." },
            { "ヌルスラ", "Nurslath - A poor island location in the Infinite Island Chain. Also known as the Last Island or Graveyard Isle." },
            { "ピルッツァ島", "Piruzza - An island in the Infinite Island Chain." }
        };

        public string Locale { get => "en"; }

        public IAsyncTextGenerator TextGenerator { get; set; }
        public IAsyncTextTransformer TextTransformer { get; set; }
        public int MaxDialogueHistoryCount { get; set; }

        public EnglishLocalizer(IAsyncTextGenerator textGenerator, IAsyncTextTransformer textTransformer) : this(textGenerator, textTransformer, new Options()) { }
        public EnglishLocalizer(IAsyncTextGenerator textGenerator, IAsyncTextTransformer textTransformer, Options options)
        {
            TextGenerator = textGenerator;
            TextTransformer = textTransformer;
            MaxDialogueHistoryCount = options.MaxDialogueHistoryCount;
        }

        public async Task<string> LocalizeAsync(string text, CancellationToken cancellationToken = default)
        {
            return (await LocalizeAsync(new GeneralText[] { new GeneralText() { Text = text } }, cancellationToken).ConfigureAwait(false)).First().Text;
        }

        public async Task<IEnumerable<GeneralText>> LocalizeAsync(IEnumerable<GeneralText> generalTexts, CancellationToken cancellationToken = default)
        {
            string prompt =
                $"""
                You are localizing the game Atelier Resleriana from Japanese to English.

                IMPORTANT CONTEXT:
                - Atelier Resleriana was officially localized to English up to the end of the first story arc before localization was discontinued
                - Your task is to continue the localization in a manner consistent with the official release

                LOCALIZATION GUIDELINES:
                - Adhere closely to the established localization style, tone, and conventions from the official material
                - Study and match the writing style, character voices, and terminology used in the official localization
                - Maintain consistency with established character names, terminology, and locations from previous Atelier games
                - Several characters are returning from older Atelier titles - use their official English localizations
                - Reference terms related to alchemy, synthesis, and item creation follow specific conventions in the Atelier series
                - For returning characters (Sophie, Plachta, Ramizel, etc.), maintain their established personalities and speech patterns
                - Location names often combine German/French roots with fantasy elements - preserve this naming convention
                - Technical terms for game mechanics (e.g., "Synthesis," "Quality," "Traits") have established translations
                - Preserve any cultural nuances that were intentionally kept in the official localization
                - When uncertain about a term, prioritize consistency with the existing official localization over creating new terminology

                OUTPUT GUIDELINES:
                - Your output must be pure JSON. Nothing else. No preamble, no explanations etc.
                - Your JSON schema be an array that matches the input JSON array in length.
                - Each JSON object in the array must have the schema:
                {JsonConvert.SerializeObject(new JsonSchemaConverter().Convert<GeneralText>())}

                <input>
                {JsonConvert.SerializeObject(generalTexts)}
                </input>
                """;

            GeneralText[] generalTextLocalizations = await TextTransformer.TransformAsync<GeneralText[]>(prompt, cancellationToken).ConfigureAwait(false);

            return generalTextLocalizations;
        }

        private string DialogueTextPreamblePromptFragment(IEnumerable<string> currentSpeakers)
        {
            string speakerContext = GetSpeakerSpecificNotes(currentSpeakers.ToList());

            return
                $"""
                You are localizing the game Atelier Resleriana from Japanese to English.
                
                IMPORTANT CONTEXT:
                - Atelier Resleriana was officially localized to English up to the end of the first story arc before localization was discontinued
                - Your task is to continue the localization in a manner consistent with the official release
                
                CHARACTER INFORMATION:
                {DetailedCharacterInformation}
                
                SPEAKER NOTES:
                {speakerContext}
                
                WORLD NOTES:
                {string.Join("\n", WorldContext.Select(kv => $"{kv.Key}: {kv.Value}"))}
                """;
        }

        private string LocalizationGuidelinesPromptFragment()
        {
            return
                """
                LOCALIZATION GUIDELINES:
                - Study and closely match the style, flow, and tone used in the recent dialogue context
                - Maintain strong consistency with how characters and terms are translated in preceding scenes
                - Look for patterns in how character voice and personality are expressed in recent translations
                - Adhere to established localization style, tone, and conventions from the official material
                - Preserve character-specific speech patterns and quirks (e.g., Rorona's airheaded remarks, Meruru's enthusiasm, Sterk's formality)
                - Use consistent terminology for alchemy concepts (e.g., "Synthesis," "Quality," "Traits," "Materials") and maintain the localization team's choices for world locations, concepts, and characters
                - Handle Japanese honorifics according to previous localization patterns. This usually means dropping them entirely or completely substituting it with another English title of address that matches the tone and nuance
                - Respect dialogue length limitations - try to keep text about the same length as the original
                - Newline characters you encounter in the source material should be dropped from the output - the source material does not make use of text wrapping but the localized material will
                - When uncertain about a term, check how it was handled in recent dialogue first and foremost
                - Prioritize natural English dialogue flow over literal translation
                - Favor adaptation over direct translation for cultural references and jokes
                - Reduce the frequency of character names in dialogue compared to the Japanese script
                - Use pronouns (you/he/she) more liberally when context is clear, especially in direct conversation
                - For Japanese wordplay or puns, focus on recreating the humor's spirit rather than translating literally
                - Occasionally the speaker may not be an individual but a group, in which case do not make the mistake of translating this as a name.
                - Each character must have a consistent and distinct voice that reflects their personality, infer this from previously localized dialogue and maintain this
                - Carefully observe each character's unique speech markers: Lara uses formal, aristocratic phrasing ("do you mean to become a citizen"); Izana tends toward exclamations and casual expressions; Antje speaks minimally with emotionally reserved responses.
                - When characters talk about themselves in third person (like Flocke), preserve this trait but adapt it to sound natural in English rather than awkward.
                - Pay attention to power dynamics and relationships between characters (mentor/student, friends, rivals) and ensure these are conveyed through dialogue tone and word choice.
                - Group conversations should maintain distinct voices while showing their relationships - friendly teasing, respect, or caution should come through naturally.
                - When multiple characters react to the same situation, vary their responses to reflect their personalities rather than having similar reactions.

                ADAPTATION GUIDELINES:
                - Japanese sentences often end with emotional markers that should be repositioned earlier in English sentences
                - Convert subject-object-verb Japanese structure to English's subject-verb-object for natural flow
                - Implied subjects in Japanese should be made explicit in English when necessary for clarity
                - "Talking to oneself" passages common in Japanese should be converted to more natural English inner monologue
                - Praise phrases using さすが should be localized to something appropriate for the situation in English (eg. "Nice work!", "That was amazing!", etc. or simply omitting the phrase) rather than the unnatural 'as expected of...'-type construction.
                - Preserve emotional beats and tone over literal word-for-word translation
                - Add appropriate English idioms and expressions to maintain the natural conversational feel
                - Punctuation should be professionally presented - each dialogue turn should usually end with some sort of punctuation.
                - Adapt Japanese humor contextually rather than literally. For example, when characters react with exaggerated shock ("Eeek!"), focus on conveying the emotion rather than translating the specific exclamation.

                HONORIFICS:
                - Never retain Japanese honorifics (-san, -chan, -sama, etc.) in the English localization
                - Drop the honorific entirely when the relationship is clear ("Janne" not "Janne-san")
                - Use appropriate English titles if needed ("Miss," "Master," etc.)
                - In cases of familial terms like onee-chan, use natural English equivalents only when required. It is preferably to omit the name or reference entirely if it would flow naturally in English.
                - Reduce frequency of name/title usage in dialogue - While Japanese may repeat names frequently, English typically uses pronouns or no direct reference once context is established
                - When the honorific carries crucial relationship information, convey this through dialogue tone and word choice instead

                STYLING NOTES:
                - Carefully match the writing style shown in the recent dialogue examples
                - Character personalities should be distinct but not exaggerated caricatures and should reflect their current state in the story
                - Humor is often gentle and situational rather than sarcastic
                - Official localizations often take creative liberties to make dialogue feel natural in English
                - The series has a whimsical quality that should be preserved in wordplay and expressions

                SENTENCE FRAGMENTS:
                - Convert short けど endings to complete thoughts in English
                - Expand だね fragments into full statements when needed for clarity
                - Turn verb-only responses into natural English short replies
                - Add implied subjects/objects when needed for English flow
                - Maintain the casual/formal tone even when expanding fragments

                STYLISTIC REQUIREMENTS:
                - Use ellipses (...) strategically to indicate hesitation, trailing thoughts, or dramatic pauses
                - Employ exclamation marks to convey enthusiasm or surprise, especially for characters like Izana
                - Break longer Japanese sentences into multiple English sentences when it creates better rhythm
                - Maintain the game's whimsical tone through playful language without becoming childish
                - Use contractions in casual conversation (don't, can't, I'll) but avoid them in more formal speech
                - Vary sentence length for natural dialogue rhythm - mix short, punchy statements with longer expressions
                - Make dialogue punchy and impactful when appropriate - shorter responses often have more impact in English than longer ones.
                - When adapting longer Japanese sentences, prioritize flow and impact over preserving every detail from the original.
                - Character reactions should feel natural and spontaneous, especially for exclamations and short responses.

                ADAPTATION EXAMPLES:
                Original: "頑張っていこう、明日も！" 
                ✓ "Let's keep at it tomorrow too!" 
                ✗ "Let's do our best, even tomorrow!"
                Key: Natural enthusiasm, drops formulaic phrasing

                Original: "ユーディー先輩、ユーディー先輩の錬金術すごいです！"
                ✓ "Your alchemy is amazing, Miss Judie!"
                ✗ "Senior Judie! Senior Judie's alchemy is amazing!"
                Key: Reduces name repetition, uses appropriate title

                Original: "本当に…特に愛紗には迷惑かけてばかり"
                ✓ "Yeah, I do feel like we're always asking Ayesha for medicine."
                ✗ "Really... especially to Ayesha, we're always causing trouble."
                Key: Converts formal apology to natural admission

                ADDITIONAL ADAPTATION EXAMPLES:
                CHARACTER-SPECIFIC SPEECH:
                - Original (Magda): "おうよ、できるさ。" → Good: "Heck yeah, I got this." (Not: "Yes, I can do it.")
                - Original (Lanze): "相棒は大丈夫だってよ" → Good: "My partner's tough as nails, don't you worry." (Not: "They said my partner is fine.")
                - Original (Heidi): "その辺りに気を付けなきゃね" → Good: "We'd better keep an eye out for that." (Not: "We must be careful about that area.")

                EMOTIONAL ADAPTATION:
                - Original: "えっ！？ そ、そうなの？" → Good: "Wait, seriously?!" (Not: "Eh!? I-is that so?")
                - Original: "ふふっ。誘ってくれてありがと" → Good: "Hehe. Thanks for inviting me along." (Not: "Fu fu. Thank you for inviting me.")
                - Original: "うーん…それはどうかな" → Good: "Hmm... I'm not so sure about that." (Not: "Well... how about that?")

                NATURAL DIALOGUE FLOW:
                - Original: "何かキッカケがあれば…" → Good: "If we just had one more clue..." (Not: "If there was some kind of trigger...")
                - Original: "随分、島の奥まで来ましたね" → Good: "We've made it quite a ways into the island, haven't we?" (Not: "We have considerably come to the depths of the island.")
                - Original: "理想郷を目指す、だあ？" → Good: "Huh? You're settin' out for the isle of legend? You must have a death wish!" (Not: "Aiming for utopia, huh?")

                CONCISE ADAPTATIONS:
                - Original: "異邦人たちの集うアトリエ！" → Good: "So, this is the atelier that all the wanderers have been flocking to!" (Not: "The atelier where foreigners gather!")
                - Original: "大丈夫だよ、あいつは丈夫なんだ" → Good: "She'll be fine. That one's tough." (Not: "It's okay, that person is sturdy.")
                - Original: "最初から探してもなかなか見つからんだろうから" → Good: "You won't find many ships that can make that journey." (Not: "Because from the beginning, it would be hard to find.")

                CULTURAL ADAPTATIONS:
                - Original: "お役に立てて良かったです！" → Good: "We're just happy we could help!" (Not: "I'm glad I could be of service!")
                - Original: "油断ならないわね" → Good: "I'd prefer not to let my guard down." (Not: "We cannot be careless.")
                - Original: "本当に…特に愛紗には迷惑かけてばかり" → Good: "Yeeeah, I do feel like we're always asking Ayesha for medicine." (Not: "Really... especially to Ayesha, we're always causing trouble.")

                CONVERSATIONAL EXCHANGE ADAPTATIONS:
                - Original A: "合ってる？" Original B: "ええ、その通りよ" → Good A: "Is that right?" Good B: "Spot on." (Not: "Is that correct?" "Yes, that is so.")
                - Original A: "どうしたの？" Original B: "いや、なんでもない" → Good A: "What's wrong?" Good B: "It's nothing." (Not: "What happened?" "No, it is nothing.")
                - Original A: "計画通りだな" Original B: "当然だ" → Good A: "Everything's going according to plan." Good B: "Naturally." (Not: "It is according to plan." "Of course.")

                HONORIFIC ADAPTATIONS:
                - Original: "ヤンネお姉ちゃん、待って！" → Good: "Wait up!" (Not: "Wait, Big Sis Janne!")
                - Original: "姉様、ただいま戻りました" → Good: "I'm back!" (Not: "Dear sister, I have returned.")

                AVOID THESE COMMON MISTAKES:
                - Overly literal translations that sound unnatural in English
                - Translating emotional exclamations directly rather than finding English equivalents
                - Maintaining Japanese sentence structure when it would sound awkward in English
                - Adding unnecessary explanations that weren't in the original text
                - Losing character-specific speech patterns in favor of generic dialogue
                - Using formal English for casual conversation or vice versa
                """;
        }

        private string DialogueTextSchemaPromptFragment()
        {
            return
                $"""
                OUTPUT GUIDELINES:
                - Your output must be pure JSON. Nothing else. No preamble, no explanations etc.
                - Your JSON schema be an array that matches the input JSON array in length.
                - Each JSON object in the array must have the schema:
                {JsonConvert.SerializeObject(new JsonSchemaConverter().Convert<DialogueText>())}
                """;
        }

        private string BuildDialogueHistorySection(List<LocalizedDialogueText> contextDialogues)
        {
            if (!contextDialogues.Any()) return "";

            // Take last 40 entries for recent context
            var recentDialogues = contextDialogues.Count <= 40
                ? contextDialogues
                : contextDialogues.Skip(contextDialogues.Count - 40).ToList();

            StringBuilder dialogueBuilder = new StringBuilder();
            string? currentSpeaker = null;

            foreach (var dialogue in recentDialogues)
            {
                if (dialogue.Speaker != currentSpeaker)
                {
                    dialogueBuilder.AppendLine();
                    dialogueBuilder.AppendLine($"{dialogue.Speaker} ({dialogue.LocalizedSpeaker}):");
                    currentSpeaker = dialogue.Speaker;
                }
                dialogueBuilder.AppendLine($"Original: \"{dialogue.Text}\"");
                dialogueBuilder.AppendLine($"English: \"{dialogue.LocalizedText}\"");
            }

            return dialogueBuilder.ToString();
        }

        public async Task<IEnumerable<DialogueText>> LocalizeAsync(IEnumerable<DialogueText> dialogueTexts, CancellationToken cancellationToken = default)
        {
            var currentSpeakers = GetCurrentSpeakers(dialogueTexts);

            string prompt =
                $"""
                {DialogueTextPreamblePromptFragment(currentSpeakers)}

                {LocalizationGuidelinesPromptFragment()}

                {DialogueTextSchemaPromptFragment()}

                <input>
                {JsonConvert.SerializeObject(dialogueTexts)}
                </input>
                """;

            DialogueText[] dialogueTextLocalizations = await TextTransformer.TransformAsync<DialogueText[]>(prompt, cancellationToken).ConfigureAwait(false);

            return dialogueTextLocalizations;
        }

        public async Task<IEnumerable<DialogueText>> LocalizeAsync(IEnumerable<DialogueText> dialogueTexts, IEnumerable<LocalizedDialogueText> localizedDialogueTexts, CancellationToken cancellationToken = default)
        {
            var currentSpeakers = GetCurrentSpeakers(dialogueTexts);
            var selectedSamples = SelectSamplesForDialogue(dialogueTexts, localizedDialogueTexts.ToList());
            var examples = BuildExamplesSection(currentSpeakers, selectedSamples);
            var recentDialogue = BuildDialogueHistorySection(localizedDialogueTexts.ToList());

            string prompt =
                $"""
                {DialogueTextPreamblePromptFragment(currentSpeakers)}
        
                {LocalizationGuidelinesPromptFragment()}

                {(selectedSamples.Any() ? "LOCALIZATION EXAMPLES:\n" + examples : "")}
                
                RECENT LOCALIZED DIALOGUE HISTORY:
                {recentDialogue}

                {DialogueTextSchemaPromptFragment()}

                <input>
                {JsonConvert.SerializeObject(dialogueTexts)}
                </input>
                """;

            DialogueText[] dialogueTextLocalizations =
                await TextTransformer.TransformAsync<DialogueText[]>(prompt, cancellationToken)
                    .ConfigureAwait(false);

            return dialogueTextLocalizations;
        }

        public async Task<IEnumerable<DialogueText>> LocalizeAsync(IEnumerable<DialogueText> dialogueTexts, IEnumerable<LocalizedDialogueText> localizedDialogueTexts, string storySummary, CancellationToken cancellationToken = default)
        {
            var currentSpeakers = GetCurrentSpeakers(dialogueTexts);
            var selectedSamples = SelectSamplesForDialogue(dialogueTexts, localizedDialogueTexts.ToList());
            var examples = BuildExamplesSection(currentSpeakers, selectedSamples);
            var recentDialogue = BuildDialogueHistorySection(localizedDialogueTexts.ToList());

            string prompt =
                $"""
                {DialogueTextPreamblePromptFragment(currentSpeakers)}

                STORY THUS FAR:
                {storySummary}

                {LocalizationGuidelinesPromptFragment()}

                {(selectedSamples.Any() ? "LOCALIZATION EXAMPLES:\n" + examples : "")}
                
                RECENT LOCALIZED DIALOGUE HISTORY:
                {recentDialogue}

                {DialogueTextSchemaPromptFragment()}

                <input>
                {JsonConvert.SerializeObject(dialogueTexts)}
                </input>
                """;

            DialogueText[] dialogueTextLocalizations =
                await TextTransformer.TransformAsync<DialogueText[]>(prompt, cancellationToken)
                    .ConfigureAwait(false);

            return dialogueTextLocalizations;
        }

        public async Task<string> SummarizeAsync(IEnumerable<DialogueText> dialogueTexts, CancellationToken cancellationToken = default)
        {
            StringBuilder dialogueBuilder = new StringBuilder();
            string? currentSpeaker = null;

            foreach (var dialogue in dialogueTexts)
            {
                if (dialogue.Speaker != currentSpeaker)
                {
                    if (currentSpeaker != null)
                    {
                        dialogueBuilder.AppendLine("\n");
                    }
                    dialogueBuilder.AppendLine($"{dialogue.Speaker}:");
                    currentSpeaker = dialogue.Speaker;
                }
                dialogueBuilder.AppendLine($"  \"{dialogue.Text}\"");
            }

            string prompt =
                $"""
                Your task is to create a story summary of recent events in the JRPG Atelier Resleriana based on dialogue exchanges.

                This summary will be used directly in the "STORY THUS FAR:" section of a localization document, so begin your text immediately with the narrative content.

                Create a comprehensive summary that:
                - Describes major plot developments and conflicts
                - Explains character relationships and motivations
                - Identifies key emotional moments and revelations
                - Highlights worldbuilding elements relevant to upcoming scenes
                - Is around 500 words of concise, clear narrative in past tense. 
                - Begin your response immediately with the summary, do not include any introductory phrases like "Here is a summary" or "The story is about" and do not acknowledge these instructions.
                - Focus on providing context that would help translators understand character dynamics and plot points relevant to future scenes.

                <input>
                {dialogueBuilder}
                </input>
                """;

            return await TextGenerator.GenerateAsync(prompt, cancellationToken).ConfigureAwait(false);
        }

        // Helper method to find conversations in the examples
        private List<List<LocalizedDialogueText>> FindConversations(List<LocalizedDialogueText> examples, int minLength, int maxLength)
        {
            List<List<LocalizedDialogueText>> conversations = new List<List<LocalizedDialogueText>>();

            for (int i = 0; i < examples.Count - minLength + 1; i++)
            {
                // Try to identify a sequence with multiple speakers
                HashSet<string> speakers = new HashSet<string>();
                List<LocalizedDialogueText> conversation = new List<LocalizedDialogueText>();

                for (int j = 0; j < maxLength && i + j < examples.Count; j++)
                {
                    var example = examples[i + j];
                    conversation.Add(example);
                    speakers.Add(example.Speaker);

                    // If we have at least minLength examples and 2+ speakers, we have a conversation
                    if (j + 1 >= minLength && speakers.Count >= 2)
                    {
                        conversations.Add(new List<LocalizedDialogueText>(conversation));
                        break;
                    }
                }
            }

            // Sort by number of distinct speakers (more is better)
            return conversations
                .OrderByDescending(c => c.Select(d => d.Speaker).Distinct().Count())
                .ToList();
        }

        // Helper to provide speaker-specific notes
        private string GetSpeakerSpecificNotes(List<string> speakers)
        {
            StringBuilder notesBuilder = new StringBuilder("SPECIFIC SPEAKER NOTES:\n");
            bool addedNotes = false;

            foreach (var speaker in speakers)
            {
                if (SpeakerNotes.ContainsKey(speaker))
                {
                    notesBuilder.AppendLine($"- {speaker}: {SpeakerNotes[speaker]}");
                    addedNotes = true;
                }
            }

            return addedNotes ? notesBuilder.ToString() : "";
        }

        private string BuildExamplesSection(IEnumerable<string> currentSpeakers, IEnumerable<LocalizedDialogueText> localizedDialogueTexts)
        {
            var examplesBySpeaker = localizedDialogueTexts
                .GroupBy(l => l.Speaker)
                .ToDictionary(g => g.Key, g => g.ToList());

            StringBuilder examplesBuilder = new StringBuilder();
            foreach (var speaker in currentSpeakers)
            {
                if (examplesBySpeaker.ContainsKey(speaker))
                {
                    examplesBuilder.AppendLine($"EXAMPLES FOR {speaker}:");
                    foreach (var example in examplesBySpeaker[speaker].Take(3)) // Limit to 3 examples per speaker
                    {
                        examplesBuilder.AppendLine($"Original: \"{example.Speaker}: {example.Text}\"");
                        examplesBuilder.AppendLine($"English: \"{example.LocalizedSpeaker}: {example.LocalizedText}\"");
                        examplesBuilder.AppendLine();
                    }
                }
            }

            var conversations = FindConversations(localizedDialogueTexts.ToList(), 3, 6);
            if (conversations.Any())
            {
                examplesBuilder.AppendLine("CONVERSATION EXAMPLES:");
                foreach (var conversation in conversations.Take(2)) // Limit to 2 conversations
                {
                    examplesBuilder.AppendLine("Original conversation:");
                    foreach (var utterance in conversation)
                    {
                        examplesBuilder.AppendLine($"  {utterance.Speaker}: {utterance.Text}");
                    }

                    examplesBuilder.AppendLine("English translation:");
                    foreach (var utterance in conversation)
                    {
                        examplesBuilder.AppendLine($"  {utterance.LocalizedSpeaker}: {utterance.LocalizedText}");
                    }
                    examplesBuilder.AppendLine();
                }
            }

            return examplesBuilder.ToString();
        }

        private List<string> GetCurrentSpeakers(IEnumerable<DialogueText> dialogueTexts)
        {
            return dialogueTexts.Select(d => d.Speaker).Distinct().ToList();
        }

        private List<List<LocalizedDialogueText>> FindContiguousConversations(
            List<LocalizedDialogueText> officialLocalizations,
            HashSet<string> relevantSpeakers)
        {
            List<List<LocalizedDialogueText>> conversations = new List<List<LocalizedDialogueText>>();

            for (int i = 0; i < officialLocalizations.Count - 2; i++)
            {
                List<LocalizedDialogueText> conversation = new List<LocalizedDialogueText>();
                bool hasRelevantSpeaker = false;

                for (int j = 0; j < 8 && i + j < officialLocalizations.Count; j++)
                {
                    var entry = officialLocalizations[i + j];
                    conversation.Add(entry);

                    if (relevantSpeakers.Contains(entry.Speaker))
                    {
                        hasRelevantSpeaker = true;
                    }
                }

                if (conversation.Count >= 3 && hasRelevantSpeaker)
                {
                    int speakerCount = conversation.Select(c => c.Speaker).Distinct().Count();

                    if (speakerCount >= 2)
                    {
                        conversations.Add(conversation);
                        i += conversation.Count - 1;
                    }
                }
            }

            return conversations
                .OrderByDescending(c =>
                    c.Count(d => relevantSpeakers.Contains(d.Speaker)) * c.Count)
                .ToList();
        }

        private List<LocalizedDialogueText> SelectSamplesForDialogue(
            IEnumerable<DialogueText> dialogueTexts,
            List<LocalizedDialogueText> contextDialogues)
        {
            HashSet<string> currentSpeakers = new HashSet<string>(
                dialogueTexts.Select(d => d.Speaker).Distinct()
            );

            Dictionary<string, List<LocalizedDialogueText>> speakerExamples =
                contextDialogues
                    .Where(l => currentSpeakers.Contains(l.Speaker))
                    .GroupBy(l => l.Speaker)
                    .ToDictionary(g => g.Key, g => g.ToList());

            List<LocalizedDialogueText> selectedSamples = new List<LocalizedDialogueText>();

            // First pass: Get at least one sample per speaker if available
            foreach (string speaker in currentSpeakers)
            {
                if (speakerExamples.ContainsKey(speaker) && speakerExamples[speaker].Count > 0)
                {
                    selectedSamples.Add(speakerExamples[speaker].Last());
                }
            }

            // Second pass: Find conversations
            List<List<LocalizedDialogueText>> conversations = FindContiguousConversations(contextDialogues, currentSpeakers);

            foreach (var conversation in conversations.Take(3))
            {
                foreach (var dialogueText in conversation)
                {
                    if (!selectedSamples.Any(s => s.Speaker == dialogueText.Speaker && s.Text == dialogueText.Text))
                    {
                        selectedSamples.Add(dialogueText);
                    }
                }
            }

            // Third pass: Fill quota based on speaker frequency
            var speakerFrequency = dialogueTexts
                .GroupBy(d => d.Speaker)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(kvp => kvp.Value);

            int targetSampleCount = Math.Min(20, Math.Max(10, dialogueTexts.Count() / 10));

            foreach (var kvp in speakerFrequency)
            {
                if (selectedSamples.Count >= targetSampleCount) break;

                string speaker = kvp.Key;
                if (!speakerExamples.ContainsKey(speaker)) continue;

                int currentCount = selectedSamples.Count(s => s.Speaker == speaker);
                int desiredSamples = Math.Min(3, kvp.Value / 5) - currentCount;

                if (desiredSamples > 0)
                {
                    var additionalSamples = speakerExamples[speaker]
                        .Where(l => !selectedSamples.Any(s =>
                            s.Speaker == l.Speaker && s.Text == l.Text))
                        .OrderByDescending(l => l.Text.Length)
                        .Take(desiredSamples);

                    selectedSamples.AddRange(additionalSamples);
                }
            }

            // Log info about selected samples
            Console.WriteLine($"Selected {selectedSamples.Count} samples for current dialogue");
            Console.WriteLine($"Sample speakers: {string.Join(", ", selectedSamples.Select(s => s.Speaker).Distinct())}");

            return selectedSamples;
        }

        public async Task<IEnumerable<JsonObject>> LocalizeAsync(
            string masterDataFileName,
            IEnumerable<JsonObject> entities,
            IEnumerable<MasterDataLocalizationExample> localizationExamples,
            CancellationToken cancellationToken = default)
        {
            // Helper function to count string fields in a JsonNode
            int CountStringFields(JsonNode node)
            {
                int count = 0;
                if (node is JsonObject jsonObj)
                {
                    foreach (var prop in jsonObj)
                    {
                        if (prop.Value is JsonValue val && val.TryGetValue<string>(out _))
                            count++;
                        else if (prop.Value != null)
                            count += CountStringFields(prop.Value);
                    }
                }
                else if (node is JsonArray arr)
                {
                    foreach (var item in arr)
                    {
                        if (item != null)
                            count += CountStringFields(item);
                    }
                }
                return count;
            }

            // Determine complexity based on first entity
            var firstEntity = entities.FirstOrDefault();
            int complexity = firstEntity != null ? CountStringFields(firstEntity) : 0;

            // Determine sample size based on complexity
            int sampleSize;
            if (complexity > 5)
            {
                sampleSize = 3;
            }
            else if (complexity > 3)
            {
                sampleSize = 10;
            }
            else if (complexity > 1)
            {
                sampleSize = 20;
            }
            else
            {
                sampleSize = 40;
            }

            var selectedExamples = localizationExamples
                .Random(sampleSize)
                .ToList();

            // Build examples section if we have any
            string examplesSection = "";
            if (selectedExamples.Any())
            {
                examplesSection =
                    $"""
                    LOCALIZATION EXAMPLES:
                    The following examples show how similar content has been localized:

                    {System.Text.Json.JsonSerializer.Serialize(selectedExamples, new System.Text.Json.JsonSerializerOptions()
                    {
                        WriteIndented = true
                    })}
                    """;
            }

            // Include an optional section for specific files.
            string contextPrompt =
                $$"""
                TECHNICAL TERMINOLOGY

                Elements/Affinities (属性/得意属性):
                火属性 - Fire
                氷属性 - Ice  
                雷属性 - Bolt
                風属性 - Air
                斬属性 - Slash
                打属性 - Strike
                突属性 - Stab
                全属性 - All (Elements)

                Core Battle Concepts:
                ターン - Turn: Allies and enemies take turns on a timeline in turns.
                タイムライン - Timeline: The current ordering of turns. Panels may appear on certain turns with certain effects.
                スキル - Skill: Characters have 3 skills - Skill 1, Skill 2, and a Burst Skill. Some can transform after burst.
                チャージインジケーター - Charge Indicator: Skills may have charge indicators for additional effects.
                ブレイクゲージ - Stun Gauge
                ブレイクダメージ - Stun Damage
                物理ブレイクダメージ - Physical Stun Damage
                魔法ブレイクダメージ - Magical Stun Damage
                ブレイク状態 - Stunned: a state entered by an enemy once their stun gauge gets depleted.
                与ダメージ - Damage Dealt
                被ダメージ - Damage Received
                カウンター - Counter
                与ブレイクダメージ - Stun Damage Dealt
                被ブレイクダメージ - Stun Damage Received
                単体与ダメージ - Single Target Damage Dealt
                被マイナス効果量 - Negative Effect Potency
                強制ブレイク - Force Stun: An effect that forces the break (stun) gauge to empty instantly, triggering a stun state regardless of the gauge's current value
                スキル威力 - Skill Power: Base multiplier affecting skill damage (rare).
                スキルダメージ - Skill Damage: Common multiplier affecting skill damage.
                物理攻撃 - Physical Damage
                魔法攻撃 - Magical Damage
                クリティカル確率 - Critical Rate
                クリティカルダメージ - Critical Damage
                全体攻撃 - Area Attack
                単体攻撃 - Single Target Attack

                Panel System (don't capitalize - not proper nouns):
                パネル - panel
                強化系パネル - boost-type panel
                強化パネル - boost panel
                強化+パネル - boost+ panel
                弱体パネル - weaken panel
                弱体+パネル - weaken+ panel
                バーストパネル - burst panel
                クリティカルパネル - critical panel
                ブレイク強化パネル - stun boost panel
                加護弱化パネル - protection weaken panel
                パネル生成 - (panel creation): certain skills or items can create panels on the timeline.
                パネル変換 - (panel conversion): certain skills or items can convert existing panels to panels of other types.

                Effects:
                バースト - Burst
                バフ - Buff: Positive effect granting advantages
                マイナス効果 - Negative Effect
                状態異常 - Status Effect
                重複不可 - Non-stackable
                耐性 - Resistance: Against elements or status effects

                Status Effects:
                毒状態 - Poison
                火傷状態 - Burn
                麻痺 - Paralysis
                暗闇 - Darkness
                混乱 - Confusion
                睡眠 - Sleep
                挑発 - Provoke/Taunt
                再生 - Regeneration
                スタン - Daze
                素早さダウン - Speed Down
                強化効果無効 - Nullify Positive Effects
                回復無効 - Nullify HP Recovery
                行動遅延 - Delay Turn

                Stats:
                Lv1 - Level: Use styling (Lv. 1) or (MAX Lv. 10) in the localization instead of the original Japanese styling.
                HP - HP
                物攻 - P.ATK: Physical attack stat
                魔攻 - M.ATK: Magical attack stat
                物防 - P.DEF: Physical defense stat
                魔防 - M.DEF: Magical defense stat
                速度 - SPD: Speed stat affecting turn order

                Character Roles:
                アタッカー - Attacker
                ブレイカー - Breaker  
                ディフェンダー - Defender
                サポーター - Supporter

                Battle States:
                ブレイク状態 - Stunned
                かばう - Cover

                Targeting Terms:
                敵全体 - all enemies
                味方全員 - all allies
                最大HPが最も高い - highest max HP
                現在HPが最も高い - highest current HP
                HPが最も減っている - lowest HP percentage
                物攻/魔攻が最も高い - highest P.ATK/M.ATK
                自身 - Self
                単体 - Single Target
                敵単体 - Single Enemy
                味方単体 - Single Ally
                魔攻が一番高い - Highest M.ATK
                物攻が一番高い - Highest P.ATK
                相手全員 - all enemies (alternative to 敵全体)
                相手対象 - target enemy
                仲間対象 - target ally

                Items:
                フラム - Bomb
                ルフト - Luft
                プラジグ - Plajig
                うに - Uni

                Tags:
                アカデミー - Academy
                アーランド - Arland
                コルセイト - Colseit
                キルヘン・ベル - Kirchen Bell
                クーケン島 - Kurken Island
                「声」を聴く者たち - Earnest Listener
                ランターナ - Lantarna
                正義と探求 - Justice & Exploration
                約束と協力 - Promise & Teamwork
                家族と友情 - Family & Friends
                騎士 - Knight
                学生 - Student
                商人 - Merchant
                師匠 - Mentor
                読書 - Bookworm
                料理 - Chef
                お菓子 - Sweets
                冒険 - Adventurer
                お嬢様 - Prim
                メガネ - Glasses
                季節の装い - Seasonal
                竜狩り - Dragon Slayer
                世話焼き - Nurturer
                おてんば - Spirited
                ずぼら - Carefree
                天然 - Whimsical
                マイペース - Laid-back
                真面目 - Diligent
                ひょうきん者 - Energetic
                寡黙 - Quiet
                レスレリ学園 - Resleriana Academy
                クリエイター - Creator
                錬金党 - Alchemist
                無数島群域 - Infinite Island Chain
                冥き追憶 - Dark Memories
                ハルフェン復興隊 - Hallfein Restoration Project
                新星 - Rising Star (when used as a character tag, so as to avoid confusion with Nova the location)
                ブラオール - Braulle

                Gacha (Wish):
                ガチャ - Wish: The term "Wish" is to be used instead of "gacha".
                引く - Wish: The verb "wish" is to be used instead of "pull".

                Other:
                手番 - turn (in context of delaying turns)
                ターン開始時 - at the start of turn
                X回付与 - granted X times
                確率 - chance (低確率 - low, 中確率 - medium, 高確率 - high, 超高確率 - very high)
                現在HP - Current HP
                最大HP - Max HP
                回復 - Recovery
                解除 - Remove/Clear
                個付与 - Grant X stacks
                回を付与 - Grant X times
                重複 - Stack
                物攻UP/DOWN - P.ATK Up/Down
                魔攻UP/DOWN - M.ATK Up/Down
                素早さUP/DOWN - SPD Up/Down
                メモリア - Memoria: A piece of artwork/card that can be equipped to a character that has effects and stats.
                付与率 - application chance (e.g., 付与率50% - 50% chance to apply)
                強化効果を持つ - with positive effects (as a condition)
                WEAK攻撃時 - when attacking a weakness
                得意属性 - affinity, commonly used in phrases like 得意属性が斬属性の時 (when equipped to a character with Slash affinity), 得意属性が突属性かつブレイカーの時 (when equipped to a Breaker with Stab affinity), 得意属性が雷属性の味方全員 (all allies with Bolt affinity).
                スキルランプ - charge indicators - some skills can have charge indicators which are "charged" (equivalent of the JP's skill lamp terminiology getting "lit").
                先駆け - initiative - often granted by abilities, and causes affected characters to act first.

                Templating Examples:
                得意属性が雷属性かつディフェンダーの時 - while equipped to a Defender with Bolt affinity
                単体攻撃のクリティカルダメージ+{0}% - boosts critical damage by {0}% for single target attacks
                攻撃対象がブレイク状態の時 - when attack target is stunned
                自身のHPに応じ、ダメージ+50〜150%(HP50〜100%で多いほど増加) - boosts damage by 50-150% depending on own HP (Higher boost for higher HP, range: 50-100% HP)
                攻撃後、対象に「受ける打属性ダメージ+{0}%」を3回攻撃を受けるまで付与し - after attacking, increases target's Strike damage received by {0}% for 3 attacks
                対象に「パネル無効」を1回行動終了するまで付与 - grants target panel immunity for 1 turn
                自身に「カウンター(雪抜剣)」を2回カウンターするまで付与 - grants self Counter (Snow Sword) for 2 counterattacks
                自身に「ぬくもりデュプリケイト」を付与(重複不可) - grants "Duplicate Warmth" to self. (Cannot be stacked)
                自身に「戦姫開花」が付与されていない時、「戦姫開花(Lv3)」を付与 - if Battle Maiden Bloom has not been granted, grants self Battle Maiden Bloom (Lv. 3)
                この攻撃はクリティカル確率+100% - boosts this attack's critical rate by 100%
                アイテムゲージを{1}%回復 - restores the item gauge by {1}%
                自身に「WEAK攻撃時、ブレイクダメージ+{0}%」を1回行動終了するまで付与 - boosts own stun damage by {0}% when attacking a weakness for 1 turn
                自身の位置をアウトレンジに移動 - shifts to the outer range
                自身の位置をインレンジに移動 - shifts to the inner range
                自身がバーストパネル獲得時 - when using a burst panel
                
                NB - Abilities with an ID range of 500000 are global abilities rather than equipment abilities, in this case the templating should be simplified (drop the "While/when equipped to" part, and just make it straightforward such as "Boost critical damage by +{0}% for characters with Fire affinity." or similar).
                NB - Not all 「」 elements get preserved - those referencing basic mechanics like skill damage and critical damage always get dropped and rewritten. The existing localization prefers to drop the punctuation completely but sometimes keeps double quotes when referring to specific skills or abilities. Prefer to drop them going forwards.
                NB - For skills with multiple parts, always end the fragments with proper punctuation, usually a period (.).

                CHARACTERS
                {{DetailedCharacterInformation}}

                MEMORIA (AND ABILITIES)
                想力「」 - Imagination ""

                WORLD
                {{string.Join("\n", WorldContext.Select(kv => $"{kv.Key}: {kv.Value}"))}}
                """;

            string prompt =
                $"""
                You are localizing game data from Japanese to English for Atelier Resleriana. The current file being processed is: {masterDataFileName}

                LOCALIZATION CONTEXT:
                - This task involves localizing structured game data, maintaining exact schema/format while translating content
                - You must preserve all IDs, numeric values, and non-text fields exactly as they are. Ensure that the keys of the JSON object are all preserved or the result will be rejected.
                - Only focus on string fields, which usually contain Japanese content. If the string field is empty, leave it as such.
                - If a string field is present and has English content, preserve it - even if it doesn't quite make sense - the original localization team did the same.

                TECHNICAL REQUIREMENTS:
                - Output must be a JSON array of JSON objects matching the input length exactly
                - Each object must maintain the exact same schema as input on an object-by-object basis.
                - Preserve all IDs, numbers, dates, and non-text data exactly

                TERMINOLOGY GUIDELINES:
                - Use consistent terminology from the Atelier series
                - Maintain established character name translations
                - Keep technical terms like "Synthesis", "Quality", "Traits" as per series standards
                - Game mechanic terms have specific official translations

                CONTENT GUIDELINES:
                - Maintain the whimsical, positive tone of the Atelier series
                - Character titles should reflect personality traits
                - Skill and ability names should be evocative but clear
                - Item descriptions should be both functional and charming
                - Quest text should motivate while clearly stating objectives

                {contextPrompt}

                {examplesSection}

                OUTPUT GUIDELINES:
                - You must produce JSON only. Do not add any other text in your response either before or after.
                - Your response must be a JSON array.
                - Each object in the JSON array must be an object with the same schema as the matching index in the original input array.

                <input>
                {System.Text.Json.JsonSerializer.Serialize(entities, new System.Text.Json.JsonSerializerOptions()
                {
                    WriteIndented = true
                })}
                </input>
                """;

            object[] localizedEntitiesNewtonsoft = await TextTransformer.TransformAsync<object[]>(prompt, cancellationToken)
                .ConfigureAwait(false);

            // Convert to string with Newtonsoft, preserving all data
            string jsonString = JsonConvert.SerializeObject(localizedEntitiesNewtonsoft, new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                DateParseHandling = DateParseHandling.None  // Prevents date parsing which could alter the data
            });

            JsonObject[] localizedEntities = JsonNode.Parse(jsonString)?.AsArray().Select(node => node!.AsObject()).ToArray()
                ?? Array.Empty<JsonObject>();

            if (localizedEntities.Length != entities.Count())
            {
                throw new InvalidOperationException(
                    $"Localization returned {localizedEntities.Length} objects but expected {entities.Count()}");
            }

            return localizedEntities;
        }

        public class Options
        {
            public int MaxDialogueHistoryCount { get; set; } = 100;
        }
    }
}
