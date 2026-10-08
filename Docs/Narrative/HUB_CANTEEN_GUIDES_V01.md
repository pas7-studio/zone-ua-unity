# Zone-UA — Canteen/guide dialogues: focused addon v0.1

Status: **PROPOSED**. Companion to `HUB_AMBIENT_LIBRARY_V01.md`; not a demand for new bar building or fixed NPC roster. The existing `dido_canopy` is a tea/fire/kitchen social anchor. A small serving counter or hot-food service could be staffed by a cook/runner (**Термос**, proposed). **Дідо is not reduced to a bartender**, and `Процент` is an occasional buyer, not a resident vendor. If no service NPC is approved, reassign mundane activity to a generic kitchen hand, *not* all lines to Dido.

## B01–B08: food, shared tables, serving counter

### B01. «М'яса нема» — dido_canopy / =2 / [R]
- **Термос:** Є гречка.
- **Буряк:** А до гречки?
- **Термос:** Ложка.
- **Буряк:** Учора з м'ясом була.
- **Термос:** Учора я її з м'ясом варив.
One dry joke; more normal daily fallback: «Сьогодні без м'яса. Постачання не прийшло».

### B02. «Тихіше» — serving_counter / =2 / [1x]
Термос витирає стіл. Новачок несе повну миску й ставить її на край.
- **Термос:** Посунь ближче.
- **Льоня:** Та не впаде.
[Миска помітно зміщується, Льоня ловить її.]
- **Термос:** Добре. Значить, руки є.
No contrived accident every loop.

### B03. «Один суп на двох» — dido_canopy / =2 / [1x], private
- **Руда:** Я заплачу за два.
- **Термос:** Він просив один.
- **Руда:** Ти йому скажи, що зайвий зварив.
- **Термос:** Він образиться.
- **Руда:** То скажи, що я образилась.
This should be two characters at the counter; referenced third person is offscreen. No reward.

### B04. «Свідок» — serving_counter / =3 / [1x]
- **Буряк:** Я вчора за чай заплатив.
- **Термос:** За вчорашній. Сьогодні новий.
- **Лис:** Я бачив, як він платив. Але не знаю, за який.
- **Буряк:** Ти міг просто підтвердити.
- **Лис:** Я це й зробив.
Not an actual currency deduction unless service mechanic exists.

### B05. «Чотири дороги» — canopy / =4 / [1x], contested rumor
- **Лис:** Мій водій до старого мосту дійшов і повернув.
- **Буряк:** Бо там Blood.
- **Руда:** А мені сказали, що міст просів.
- **Термос:** У мене від нього повідомлення є.
- **Лис:** І?
- **Термос:** «Повертаюсь». Більше нічого.
Do not force the game to decide if Blood or infrastructure caused retreat.

### B06. «Чужа вечеря» — canopy / =4 / [1x], social etiquette
[Відвідувач бере миску з чужого місця.]
- **Дідо:** Ти чию взяв?
- **Відвідувач:** Думав, загальна.
- **Термос:** Загальна в каструлі.
- **Руда:** Постав назад, я йому нову насиплю.
Short mild conflict, no humiliating punchline.

### B07. «Картопля» — canopy / =3 / [R], gentle
- **Термос:** Хто чиститиме?
- **Буряк:** Я щойно з рейду.
- **Дідо:** І руки привіз?
- **Буряк:** Привіз.
- **Термос:** Тоді мий.
Animate physical labor; next visit potatoes can actually be served. Repeats with alternate cast, not same punchline.

### B08. «Два дзвінки» — canopy / =2 / [P: 23feb]
- **Термос:** Ти їсти будеш?
- **Касир:** Пізніше.
- **Термос:** Вже третій раз кажеш.
- **Касир:** Дзвінка чекаю.
Термос мовчки лишає тарілку під кришкою. After 24.02 the table setting may persist without Kasyr. No war speech.

## G01–G06: guide knowledge, flaws and ethical choices

### G01. «Яка дорога краща?» — outer_ring / =2 / [R]
- **Льоня:** Коротка є?
- **Гугл:** Є.
- **Льоня:** То чому не нею?
- **Гугл:** Бо ти питаєш про коротку, а повернутись хочеш цілим.
Use rarely; avoid generic mentor catchphrase. Not an invitation to an unimplemented path.

### G02. «Кому вірити» — dido_canopy / =3 / [1x]
- **Лис:** Той хлопець на сітці сказав, що в низині сухо.
- **Гугл:** Коли він там був?
- **Лис:** Позавчора.
- **Руда:** То дощу ще не було.
Гугл лише киває. **Lore:** dates and weather change route usefulness.

### G03. «Біля берези» — outer_ring / =2 / [1x]
- **Руда:** Ти мітку на дереві переставив?
- **Гугл:** Так.
- **Руда:** Міг попередити.
- **Гугл:** Ти ще вчора повернулась.
- **Руда:** Я завтра знов піду.
- **Гугл:** Тому зараз і кажу.
If gameplay has dynamic markers, make physical follow-up; otherwise pure human exchange, no false world prop.

### G04. «Третій у групі» — near raid trail / =3 / [1x]
- **Лис:** Він іде зі мною.
- **Гугл:** У тебе вже двоє.
- **Лис:** Він свої ноги має.
- **Гугл:** А на вузькому переході ти своїм голосом усіх трьох вести будеш.
Suggests crowd sizes change safety; **not** mandatory formation AI.

### G05. «Назад» — near raid trail / =2 / [1x], relationship
- **Гугл:** Ти повернувся не тим ходом.
- **Лис:** Тим, яким зміг.
- **Гугл:** Хто ще знає?
- **Лис:** Ніхто.
- **Гугл:** Пішли, покажеш на карті.
No shame monologue; guide cares about future safety.

### G06. «Чому мовчав?» — outer_ring / =4 / [1x]
- **Буряк:** На повороті ж щось тріщало. Чого не сказав?
- **Лис:** Бо не знав що.
- **Льоня:** Я думав, це гілля.
- **Гугл:** Наступного разу скажи, що чуєш. Не обов'язково знати причину.
Practical rule, not a tutorial interruption.

## Two additional cross-anchor microchains

### C09. «Борщ під розпис» — serving_counter → warehouse → serving_counter
1. =2; Термос просить привезти буряка для кухні, Семен обіцяє не дату, а «як буде машина».
2. =3 at warehouse; Буряк розвантажує не ті ящики. Термос: «Де буряк?» — Буряк: «Я тут». — Термос: «Оце і проблема».
3. Offscreen arrival resolves after delivery state. At canopy either з'являється борщ or food remains plain; Термос не бреше про поставку. One joke is allowed once, then human routine. No player fetch quest.

### C10. «Провідник вернувся сам» — raid trail → garage → canopy
1. =2 at raid trail; Лис повертається без напарника, не погоджується обговорювати це з натовпом.
2. =2 garage; Лис просить Шурупа перевірити ремінь на чужому рюкзаку. Шуруп питає лише: «Його?». Відповідь: «Так».
3. Branch after credible world facts: missing partner found alive / unidentified body / still missing. An unverified corpse never becomes this person's identity.
4. canopy next visits: social follow-up is one line, gesture or silence, not a renewed investigation every day.
**Design value:** the most painful story can be told with two objects, three phrases and a changed social pattern.

## Notes

These scenes complement, rather than replace, the 30 authored A-scenes, eight C-chains and 42 S-seeds of the main library. **Cast/cardinality is strict:** an =2 confession must not fire with a 4-person cluster and a =4 retelling should not fire with two present. Characters may attend without speaking only if the script explicitly reserves them as silent participants. Do not run B01/B04 sales chatter during the locked first arrival, important authored interactions or 24.02 anchor.
