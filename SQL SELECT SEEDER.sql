INSERT INTO Items VALUES
(N'Чайник',N'Хромиран чайник създаден през 1945г.',        50.10 ,'https://www.shopixx.eu/media/t43s-4/49853.webp' , 'https://ariete.bg/wp-content/uploads/2024/06/elektricheska-kana-chajnik-vintage-1-7-l-celeste-1.jpg|https://s13emagst.akamaized.net/products/39330/39329070/images/res_db225b5def01ed0a2412ec1223164ded.jpg?width=720&height=720&hash=711AD4A44202AF1441D1287AB52EE795'),
(N'Бинокъл',N'Бинокъл използван през WW2',        150.60 ,'https://www.militarysurplus.eu/hpeciai/2031351fcb3fd06848d8a1b979b6c0bc/eng_pm_Vintage-Binoculars-with-Neck-Strap-and-Eyepiece-Cover-%E2%80%93-%D0%91%D0%9F%D0%A64-8x30-%E2%80%93-USSR-%E2%80%93-Rare-%E2%80%93-Collectible-%E2%80%93-Military-Surplus-from-the-Romanian-Army-%E2%80%93-In-Good-Condition-59138_1.jpg' , 'https://i.ebayimg.com/images/g/3ogAAOSwZC5oKwUR/s-l1600.webp|https://u-mercari-images.mercdn.net/photos/m36392337775_1.jpg|https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRQ0DmF0jykTKceNdfBpEJ2_04sK3EreCrZNw&s')

UPDATE Items
SET IsDeleted=0
WHERE Id<=2