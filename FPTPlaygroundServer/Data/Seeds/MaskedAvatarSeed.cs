﻿using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Data.Entities;
using Microsoft.Extensions.Options;

namespace FPTPlaygroundServer.Data.Seeds;

public class MaskedAvatarSeed
{
    private static IWebHostEnvironment _env = default!;
    private static IOptions<GoogleStorageSettings> _googleStorageSettings = default!;

    public MaskedAvatarSeed(IWebHostEnvironment env, IOptions<GoogleStorageSettings> googleStorageSettings)
    {
        _env = env;
        _googleStorageSettings = googleStorageSettings;
    }

    private static bool IsProduction()
    {
        if (_env.IsProduction())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<MaskedAvatar> GetDefault()
    {
        string myBucket = _googleStorageSettings.Value.Bucket;
        string folderPath = IsProduction() ? "Production" : "Development";
        return
        [
            new()
            {
                Id = Guid.NewGuid(),
                MaskedTitle = "Cyborg Warrior",
                MaskedName = "BladeX",
                MaskedDescription = "Chiến binh cyborg với mặt nạ nửa khuôn mặt, mắt đỏ rực, bộ giáp đen kim loại với vết trầy xước chiến đấu.",
                MaskedDescriptionEN = "Cyborg warrior with half face mask, glowing red eyes, black metallic armor with battle scratches.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/blade-x.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Nova Warrior",
                MaskedName = "NovaStriker",
                MaskedDescription = "Nữ chiến binh mặc giáp bạc ánh xanh, đeo kính AR phát sáng và có cánh tay máy cầm kiếm năng lượng.",
                MaskedDescriptionEN = "The female warrior wears silver-blue armor, wears glowing AR glasses, and has a mechanical arm holding an energy sword.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/nova-striker.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Unknown Hacker",
                MaskedName = "GhostZero",
                MaskedDescription = "Hacker bí ẩn với mặt nạ LED thay đổi biểu cảm, áo hoodie dài và găng tay điện tử.",
                MaskedDescriptionEN = "Mysterious hacker with expression-changing LED mask, long hoodie and electronic gloves.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/ghost-zero.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Iron Warrior",
                MaskedName = "IronFang",
                MaskedDescription = "Chiến binh có nửa cơ thể là máy móc, mặc giáp hạng nặng và có móng vuốt kim loại.",
                MaskedDescriptionEN = "A warrior with a half-machine body, wearing heavy armor and metal claws.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/iron-fang.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Storm King",
                MaskedName = "StormRider",
                MaskedDescription = "Chiến binh cưỡi mô tô bay, mặc giáp đen bạc với họa tiết sét đánh và mũ bảo hiểm phản quang.",
                MaskedDescriptionEN = "The warrior rides a flying motorcycle, wears black and silver armor with lightning motifs and a reflective helmet.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/stomp-rider.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Vortex Pilot",
                MaskedName = "Vortex",
                MaskedDescription = "Cựu phi công chiến đấu với bộ đồ phi hành gia công nghệ cao, cánh tay robot và mũ bảo hiểm kính đen.",
                MaskedDescriptionEN = "Former fighter pilot with high-tech spacesuit, robotic arms and dark visor helmet.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/vortex.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Cyber Ronin",
                MaskedName = "CyberRonin",
                MaskedDescription = "Samurai tương lai với giáp công nghệ cao, thanh kiếm plasma và mũ kabuto cách tân.",
                MaskedDescriptionEN = "Futuristic samurai with high-tech armor, plasma swords and innovative kabuto helmets.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/cyber-ronin.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Shadow Ninja",
                MaskedName = "ShadowByte",
                MaskedDescription = "Ninja trong thế giới số hóa, toàn thân bọc trong vải tối với cặp mắt phát sáng xanh lục.",
                MaskedDescriptionEN = "Ninja in the digital world, body wrapped in dark cloth with green glowing eyes.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/shadow-byte.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Titan Machine",
                MaskedName = "TitanCore",
                MaskedDescription = "Người khổng lồ bán máy móc với áo giáp titan và lõi năng lượng rực sáng giữa ngực.",
                MaskedDescriptionEN = "A semi-mechanical giant with titanium armor and a glowing energy core in the middle of his chest.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/titan-core.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Prime Leader",
                MaskedName = "OmegaPrime",
                MaskedDescription = "Thủ lĩnh của nhóm, mặc giáp trắng viền xanh, mắt phát sáng tím và mang theo khẩu súng plasma cỡ lớn.",
                MaskedDescriptionEN = "The leader of the group, wearing white armor with blue trim, eyes glowing purple and carrying a large plasma gun.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/omega-prime.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Dragon Guardian",
                MaskedName = "Seiryu",
                MaskedDescription = "Một thần rồng mạnh mẽ bảo vệ sự cân bằng giữa âm và dương. Hình dạng con người là một chiến binh khoác áo giáp xanh ngọc, mang thanh đao rồng phát sáng.",
                MaskedDescriptionEN = "A powerful dragon god who protects the balance between yin and yang. His human form is that of a warrior clad in jade-green armor, wielding a glowing dragon blade.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/seiryu.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Fire Fox",
                MaskedName = "Akane",
                MaskedDescription = "Một yêu hồ chín đuôi mang sắc đỏ rực, có khả năng điều khiển lửa ma thuật.",
                MaskedDescriptionEN = "A crimson nine-tailed fox demon with the ability to control magical fire.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/akane.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Thunder God",
                MaskedName = "Raijin",
                MaskedDescription = "Một vị thần chiến tranh mang theo trống sấm và vũ khí điện từ.",
                MaskedDescriptionEN = "A war god wielding thunder drums and electromagnetic weapons.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/raijin.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Snow Goddess",
                MaskedName = "Yuki-Onna",
                MaskedDescription = "Một linh hồn lạnh lẽo, luôn khoác trên mình bộ kimono trắng như tuyết.",
                MaskedDescriptionEN = "A cold soul, always wearing a snow-white kimono.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/yuki-onna.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Samurai Ghost",
                MaskedName = "Kuronaga",
                MaskedDescription = "Một samurai đã chết nhưng linh hồn vẫn lang thang tìm kiếm danh dự.",
                MaskedDescriptionEN = "A samurai is dead but his soul still wanders in search of honor.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/kuronaga.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Lady of Light",
                MaskedName = "Hikari",
                MaskedDescription = "Một pháp sư mạnh mẽ có thể thanh tẩy linh hồn và trừ tà.",
                MaskedDescriptionEN = "A powerful shaman who can purify souls and exorcise evil.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/hikari.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "King of Darkness",
                MaskedName = "Orochi",
                MaskedDescription = "Hình dạng con người là một nam nhân cao lớn với mái tóc dài đen tuyền, mang vảy rắn.",
                MaskedDescriptionEN = "The human form is a tall male with long jet black hair and snake scales.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/orochi.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Catwoman",
                MaskedName = "Nekomata",
                MaskedDescription = "Trong hình dạng người, là một thiếu nữ mặc yukata với đôi tai mèo, mắt vàng sáng rực.",
                MaskedDescriptionEN = "In human form, is a young girl wearing a yukata with cat ears, bright yellow eyes.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/nekomata.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Shadow Ghost",
                MaskedName = "Shinkiro",
                MaskedDescription = "Cơ thể mờ ảo, đôi mắt không có con ngươi, khoác áo choàng tím thần bí.",
                MaskedDescriptionEN = "Translucent body, pupilless eyes, wearing a mysterious purple cloak.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/shinkiro.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "God of War",
                MaskedName = "Tengu",
                MaskedDescription = "Một chiến binh nửa người nửa chim, có đôi cánh khổng lồ và chiếc mũi dài đặc trưng.",
                MaskedDescriptionEN = "A half-human, half-bird warrior with giant wings and a distinctive long nose.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/tengu.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Dragon Emperor",
                MaskedName = "Liu Bei",
                MaskedDescription = "Một hoàng đế nhân từ, mang trong mình dòng máu rồng, đại diện cho vận mệnh thiên tử.",
                MaskedDescriptionEN = "A benevolent emperor, carrying the blood of the dragon, representing the destiny of heaven.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/liu-pei.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Dragon Azure",
                MaskedName = "Guan Yu",
                MaskedDescription = "Một chiến thần bất bại, sở hữu thanh long đao huyền thoại.",
                MaskedDescriptionEN = "An undefeated war god, possessing the legendary dragon blade.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/guan-yu.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Black Tiger Beast",
                MaskedName = "Zhang Fei",
                MaskedDescription = "Một chiến binh dũng mãnh, được ví như hổ dữ nơi chiến trường.",
                MaskedDescriptionEN = "A brave warrior, likened to a fierce tiger on the battlefield.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/zhang-fei.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Divine Strategist",
                MaskedName = "Zhuge Liang",
                MaskedDescription = "Một nhà chiến lược thiên tài, được thiên hạ tôn xưng là thần mưu.",
                MaskedDescriptionEN = "A genius strategist, revered by the world as a god of strategy.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/zhuge-liang.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Black Dragon Emperor",
                MaskedName = "Cao Cao",
                MaskedDescription = "Một đế vương sắc sảo, mang trong mình sức mạnh của hắc long.",
                MaskedDescriptionEN = "A shrewd emperor, carrying the power of the black dragon.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/cao-cao.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Bewitching Beauty",
                MaskedName = "Diao Chan",
                MaskedDescription = "Một mỹ nhân sắc nước hương trời, nhưng mang sức mạnh điều khiển lòng người.",
                MaskedDescriptionEN = "A beautiful woman, but has the power to control people's hearts.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/diao-chan.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Battle God",
                MaskedName = "Lu Bu",
                MaskedDescription = "Chiến thần vô địch, không ai có thể địch nổi sức mạnh của hắn.",
                MaskedDescriptionEN = "Invincible God of War, no one can match his strength.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/lu-bu.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Dragon Commander",
                MaskedName = "Zhou Yu",
                MaskedDescription = "Một danh tướng cầm quân tài ba, mang trong mình sức mạnh của lửa và nước.",
                MaskedDescriptionEN = "A talented general, possessing the power of fire and water.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/zhou-yu.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Tiger King",
                MaskedName = "Sun Quan",
                MaskedDescription = "Ánh mắt sắc bén như loài mãnh thú săn mồi, luôn sẵn sàng bùng nổ sức mạnh.",
                MaskedDescriptionEN = "Sharp eyes like a predator, always ready to explode with power.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/sun-quan.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Dragon Knight",
                MaskedName = "Zhao Yun",
                MaskedDescription = "Một danh tướng trung thành, có sức mạnh của bạch long bảo vệ.",
                MaskedDescriptionEN = "A loyal general, protected by the power of the white dragon.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/zhao-yun.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Moon Mage",
                MaskedName = "Felix",
                MaskedDescription = "Một cậu bé pháp sư với áo choàng tím lấp lánh và chiếc đũa phép hình mặt trăng. Mỗi khi vung đũa, phép thuật ánh trăng lan tỏa, chiếu sáng màn đêm.",
                MaskedDescriptionEN = "A young wizard with a sparkling purple robe and a moon-shaped wand. Every time he waves his wand, moonlight magic spreads, illuminating the night.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/felix.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Ocean Princess",
                MaskedName = "Luna",
                MaskedDescription = "Một nàng tiên cá xinh đẹp với mái tóc xanh biếc và vương miện san hô. Cô có khả năng điều khiển sóng biển và giao tiếp với các sinh vật đại dương.",
                MaskedDescriptionEN = "A beautiful mermaid with blue hair and a coral crown. She has the ability to control the ocean waves and communicate with ocean creatures.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/luna.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Thunder Warrior",
                MaskedName = "Bolt",
                MaskedDescription = "Một chiến binh robot với bộ giáp điện năng màu xanh lam. Khi chiến đấu, anh ta có thể triệu hồi những tia sét khổng lồ giáng xuống kẻ thù.",
                MaskedDescriptionEN = "A robotic warrior with blue electric armor. When in battle, he can summon giant lightning bolts to strike down on enemies.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/bolt.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Dreaming Knight",
                MaskedName = "Milo",
                MaskedDescription = "Một chàng hiệp sĩ trẻ tuổi với thanh kiếm phép thuật có thể biến đổi theo trí tưởng tượng. Dù vụng về nhưng cậu luôn tràn đầy nhiệt huyết và dũng cảm.",
                MaskedDescriptionEN = "A young knight with a magical sword that can transform according to his imagination. Although clumsy, he is always full of enthusiasm and courage.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/milo.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Enchanting Ghost",
                MaskedName = "Zara",
                MaskedDescription = "Một linh hồn bí ẩn khoác áo choàng đen, có khả năng biến mất vào bóng tối và xuất hiện bất ngờ để đánh lạc hướng đối thủ bằng ảo ảnh.",
                MaskedDescriptionEN = "A mysterious spirit cloaked in black, capable of disappearing into the shadows and appearing suddenly to distract opponents with illusions.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/zara.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Leaf Ninja",
                MaskedName = "Kiko ",
                MaskedDescription = "Một ninja nhỏ nhắn nhưng nhanh như cơn gió, sở hữu thanh kiếm tre và khả năng ngụy trang hoàn hảo giữa thiên nhiên.",
                MaskedDescriptionEN = "A small but fast ninja, with a bamboo sword and the ability to camouflage perfectly in nature.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/kiko.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Street Bear",
                MaskedName = "Bobby",
                MaskedDescription = "Một chú gấu đeo kính râm, mặc áo khoác da, chuyên chơi guitar điện trên phố. Mỗi khi chơi nhạc, âm thanh có thể khiến mọi người nhảy múa vui vẻ.",
                MaskedDescriptionEN = "A bear wearing sunglasses and a leather jacket, specializes in playing electric guitar on the street. Every time he plays music, the sound can make people dance happily.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/bobby.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Candy Witch",
                MaskedName = "Elara",
                MaskedDescription = "Một cô phù thủy nhỏ với chiếc mũ chóp cao và cây gậy kẹo phép thuật. Cô có thể biến mọi thứ thành bánh kẹo đầy màu sắc.",
                MaskedDescriptionEN = "A little witch with a top hat and a magic candy cane. She can turn everything into colorful candies.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/elara.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Muscle Robot",
                MaskedName = "Rocko",
                MaskedDescription = "Một robot to lớn với bộ khung kim loại chắc chắn, có sức mạnh vô song và trái tim nhân hậu, luôn bảo vệ bạn bè khỏi nguy hiểm.",
                MaskedDescriptionEN = "A huge robot with a sturdy metal frame, unmatched strength and a kind heart, always protecting his friends from danger.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/rocko.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Flower Tinker",
                MaskedName = "Flora",
                MaskedDescription = "Một nàng tiên nhỏ có thể khiến hoa nở rộ chỉ bằng một cái vẫy tay. Cô bay lượn khắp nơi, gieo rắc phép màu và sự sống cho thiên nhiên.",
                MaskedDescriptionEN = "A little fairy who can make flowers bloom with just a wave of her hand. She flies around, spreading magic and life to nature.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/flora.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Thunder Knight",
                MaskedName = "Ryuji",
                MaskedDescription = "Một kiếm sĩ trẻ tuổi với mái tóc trắng bù xù, mặc áo choàng xanh đen. Thanh kiếm của cậu có thể triệu hồi sấm sét và một con rồng điện khổng lồ.",
                MaskedDescriptionEN = "A young swordsman with messy white hair, wearing a blue-black cloak. His sword can summon lightning and a giant electric dragon.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/ryuji.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Angel of Light",
                MaskedName = "Hikari",
                MaskedDescription = "Một nữ chiến binh với đôi cánh ánh sáng lấp lánh, bộ giáp trắng vàng rực rỡ. Cô có thể tạo ra lưỡi kiếm ánh sáng chém tan bóng tối.",
                MaskedDescriptionEN = "A female warrior with sparkling wings of light, radiant white and gold armor. She can create a blade of light that cuts through darkness.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/hikari.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Red Phoenix",
                MaskedName = "Akihiro",
                MaskedDescription = "Một võ sĩ samurai tóc đỏ rực như lửa, khoác áo haori đỏ đen. Mỗi nhát kiếm của anh có thể tạo ra ngọn lửa phượng hoàng thiêu rụi kẻ địch.",
                MaskedDescriptionEN = "A fiery red-haired samurai wearing a red and black haori. Each swing of his sword can create phoenix flames to burn his enemies.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/akihiro.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Ice Angel",
                MaskedName = "Yumi",
                MaskedDescription = "Một cô gái lạnh lùng với mái tóc xanh băng và đôi mắt như bầu trời mùa đông. Cô sử dụng pháp thuật băng để đóng băng mọi thứ trong chớp mắt.",
                MaskedDescriptionEN = "A cold girl with ice blue hair and eyes like the winter sky. She uses ice magic to freeze everything in the blink of an eye.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/yumi.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Wolf King",
                MaskedName = "Takeshi",
                MaskedDescription = "Một chiến binh hoang dã với áo khoác lông sói đen, đôi mắt vàng sáng quắc. Anh có thể biến hình thành một con sói khổng lồ trong màn đêm.",
                MaskedDescriptionEN = "A wild warrior with a black wolf coat and bright yellow eyes. He can transform into a giant wolf in the night.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/takeshi.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Blossom Warrior",
                MaskedName = "Sakura",
                MaskedDescription = "Một nữ kiếm sĩ thanh nhã, mặc kimono hồng thêu hoa đào. Khi chiến đấu, những cánh hoa đào bay lượn theo từng nhát kiếm của cô.",
                MaskedDescriptionEN = "An elegant swordswoman wearing a pink kimono embroidered with peach blossoms. As she fights, peach blossoms flutter in the air with each swing of her sword.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/sakura.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Dark Ninja",
                MaskedName = "Kaito",
                MaskedDescription = "Một ninja bí ẩn với mặt nạ quỷ, mặc bộ đồ đen tuyền. Anh có thể di chuyển nhanh như gió và biến mất trong làn khói tím.",
                MaskedDescriptionEN = "A mysterious ninja with a demon mask, dressed in all black. He can move as fast as the wind and disappear in purple smoke.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/kaito.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Blood Demon",
                MaskedName = "Renji",
                MaskedDescription = "Một kiếm sĩ nguy hiểm với thanh kiếm nhuốm máu, khoác áo choàng đen đỏ. Khi mặt trăng đỏ xuất hiện, sức mạnh của hắn đạt đến đỉnh cao.",
                MaskedDescriptionEN = "A dangerous swordsman with a blood-stained sword, wearing a black and red cloak. When the red moon appears, his power reaches its peak.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/renji.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "River Princess",
                MaskedName = "Mizuki",
                MaskedDescription = "Một công chúa nước với mái tóc xanh biếc và đôi mắt trong veo như biển cả. Cô có thể điều khiển dòng nước tạo thành những con rồng nước hùng mạnh.",
                MaskedDescriptionEN = "A water princess with blue hair and eyes as clear as the ocean. She can control water to form powerful water dragons.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/mizuki.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new() {
                Id = Guid.NewGuid(),
                MaskedTitle = "Haru Archer",
                MaskedName = "Haruto",
                MaskedDescription = "Cậu cung thủ mạnh mẽ, khoác lên mình bộ đồ quý tộc. Cậu đã được nhà vua nhận nuôi và trở thành hoàng tử.",
                MaskedDescriptionEN = "The strong archer, dressed in noble clothes. He was adopted by the king and became a prince.",
                AvatarUrl = $"https://storage.googleapis.com/{myBucket}/{folderPath}/masked-avatar/haruto.webp",
                Status = MaskedAvatarStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
        ];
    }
}
