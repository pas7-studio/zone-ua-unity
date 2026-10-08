# Zone-UA — Quest Lines, Contracts & Reusable Mechanics v0.4

## 1. Content layers

У грі є три типи narrative content:

### Story / Character Quests
Ручні квести з конкретними персонажами, сценами, наслідками та world-state змінами.

### Contracts
Повторювані або напівпроцедурні роботи. Гроші, ресурси, репутація, unlock-и.

### Opportunities
Не видаються NPC як квест. Гравець знаходить щось сам: рюкзак, КПК, слід, труп, дивний тайник, предмет. Після цього вирішує, що робити.

## 2. General rules

1. Кожен квест має існувати з причини у світі.
2. Не використовувати “kill 5 dogs” без конкретної world reason.
3. Не вимагати нову систему, якщо задача нормально вирішується чинними механіками.
4. Якщо додається нова механіка — вона повинна reuse'итись у кількох квестах.
5. Основні сюжетні квести можуть мати authored/anchored raid presets.
6. Нелінійність бажано будувати через:
   - що гравець помітив;
   - кому віддав evidence;
   - відкрив чи не відкрив контейнер;
   - які побічні лінії пройшов;
   - що знає персонаж.
7. Не кожна розвилка має блокувати контент. Часто достатньо змінити знання/діалог/нагороду.

## 3. First quest — “Стара закладка”

**Giver:** Касир  
**Purpose:** перший самостійний extraction loop.

### Dialogue

**Касир**
> Є одна проста.

*показує карту*

**Касир**
> Тут стара насосна. За нею в бетонній трубі лежить сумка.

**Герой**
> Чия?

**Касир**
> Наша.

**Герой**
> Чого сам не забрав?

**Касир**
> Бо тепер ти тут.

Пауза.

**Касир**
> Мені треба зрозуміти, як ти сам ходиш.

**Герой**
> А якщо сумки нема?

**Касир**
> Тоді не вигадуй. Подивись навколо й вертайся.

### КПК
Не exact marker. Запис:
- насосна на захід від старої просіки;
- схрон у бетонній водовідвідній трубі за будівлею.

### Optional observation
Біля схрону можуть бути:
- свіжий слід;
- зміщений камінь;
- чужий недопалок;
- сумка закрита інакше.

Квест проходиться і без цього.

### Return variant

Якщо observation знайдено:

**Герой**
> Хтось там був до мене.

**Касир**
> Звідки знаєш?

**Герой**
> Схрон закрили назад. Але не так.

Касир перестає розбирати сумку.

**Касир**
> Добре, що подивився.

Не показувати “Mystery unlocked”.

## 4. Google line — “Карта бреше”

### G-01 Стара просіка
**Mechanics:** navigation, bolts, anomaly reading, field notes.

**Гугл**
> Учора хлопці повернулися через болото. Кажуть, старий прохід закрився.

**Герой**
> І?

**Гугл**
> І я хочу знати — закрився він чи хлопці дурні.

Маршрут задається орієнтирами, а не exact GPS.

### G-02 Мітки
Перевірити старі route marks. Одну зірвало, другу хтось зняв акуратно. Поруч — чужий спосіб маркування.

### Later payoff
Гравець, який пройшов цю лінію, може впізнати схожу route logic на crash site 24.02.

## 5. Orest line — “Дані важливіші за залізо”

### O-01 Дванадцятий
Reuse'ить checkpoint radio seed.

**Орест**
> Ви вранці були на зовнішньому посту?

**Герой**
> Був.

**Орест**
> Чули про дванадцятий датчик?

**Герой**
> Щось чув.

**Орест**
> Тепер він мовчить.

Далі:
- module bay відкривали;
- накопичувач замінений/вийнятий;
- старий module можна знайти неподалік.

### O-02 Зразок із контекстом
Не “принеси артефакт”, а артефакт із конкретної області/після конкретної зміни.

### O-03 Не чіпай
Оглянути об'єкт, але не забирати. Гравець може порушити умову; квест не soft-lock'ається, але Орест реагує.

## 6. Semen line — logistics & weight

### S-01 Тара
Поставка була скинута біля старої вишки.

**Семен**
> Машина вчора не доїхала до бази. Водій ящик скинув біля старої вишки й повернув назад.

**Герой**
> Що в ящику?

**Семен**
> Моє.

**Герой**
> Дуже змістовно.

**Семен**
> А ти хочеш список по накладній чи гроші?

Heavy mission cargo.

Player choices:
- винести sealed crate;
- розкрити;
- узяти частину;
- кинути власний loot;
- повернутись вдруге.

Семен бачить, чи пломба ціла.

## 7. Shurup line — hub-changing technical quests

### SH-01 Стартер
Генератор працює нестабільно; частина освітлення моргає.

**Герой**
> Що з ним?

**Шуруп**
> Старий.

**Герой**
> Це діагноз?

**Шуруп**
> Це причина.

Пізніше:
> Стартер ще день походить. Може два. На старому лісгоспі такий самий двигун стояв.

Принесений starter змінює actual hub prop/sound/state.

### Future
- конкретна поламана зброя;
- важкий salvage;
- unlock repair tier;
- generator/lighting upgrades.

## 8. Kravets line — perimeter/state work

### K-01 Точка мовчить

**Кравець**
> На схід підеш?

**Герой**
> Можливо.

**Кравець**
> Якщо підеш — зайди на третю точку.

**Герой**
> Що там?

**Кравець**
> Не знаю. Тому й зайди.

На точці може бути:
- пошкоджена рація;
- сліди;
- мутанти;
- покинута зброя;
- нічого очевидного.

### K-02 Повернути державне
Evidence/state property можна:
- повернути Кравцю;
- віддати іншому персонажу;
- продати посереднику.
Наслідок — довіра/доступ/діалоги.

## 9. Dido opportunities

Дідо не працює як quest board.

Приклад:
- у tutorial hut optional item із “Сич”;
- пізніше ambient:
  - “Сича бачив?”
  - “Ні.”
  - “Він учора мав бути.”
  - “Знаю.”

Далі procedural world може дати:
- рюкзак;
- тіло;
- Сича живим;
- нічого.

## 10. First meaningful evidence branch

Гравець знаходить чужий маршрутний журнал / накопичувач / КПК:
- координати;
- частоти;
- аномальні поля;
- маршрути.

Не містить прямого пояснення змови.

Можливі адресати:
- Кравець → military/security interpretation;
- Орест → scientific pattern;
- Касир → social/buyer/context interpretation.

Не один “правильний” вибір. Головний сюжет продовжується, але діалоги та доступне знання відрізняються.

## 11. Introduction of Protsent

Процент не має постійного кіоску.

Після першої дивної знахідки інший NPC може згадати, що є людина, яка купує такі речі.

Через певний час у logistics area з'являється чужа машина й Процент. Це його introduction без cutscene.

## 12. Night quest — “Сигнал”

Вводить reusable mechanic: **Field Receiver**.

Нічний маршрут, аварійний сигнал, ліхтар, flare gun, mutants.

Field Receiver:
- strength;
- periodic beep;
- approximate sector;
- без exact minimap arrow.

Reuse:
- distress beacons;
- КПК;
- caches;
- Orest sensors;
- UAV;
- military radios;
- story transmitters.

У “Сигналі” гравець доходить до маяка, але людини немає. Є кров/сліди/вирваний модуль.

## 13. 24.02 Anchored Raid — “Падіння”

Не random procedural raid.

Preset:
- forest;
- ранній ранок;
- приглушена/похмура погода;
- 0 wandering stalkers;
- майже повна suppression fauna;
- 0 random combat events;
- authored crash-site POI;
- extraction/return тільки після investigation milestone;
- death reloads expedition checkpoint, а не продовжує timeline із hub respawn.

### Investigation beats
1. уламки в деревах;
2. trace path;
3. crash site;
4. interactive components;
5. emergency beacon через Field Receiver;
6. бортовий модуль даних;
7. missing component / чужі сліди.

Якщо гравець знає route marks із Google line — може впізнати патерн. Інакше бачить лише “чужі сліди”.

## 14. Kasyr’s last job

Після повернення хаб уже змінився. Касир пакується.

**Герой**
> Куди зібрався?

**Касир**
> На виїзд.

**Герой**
> Куди?

**Касир**
> Ти серйозно?

Пауза.

**Касир**
> На схід. Куди ще.

**Герой**
> Я з тобою.

**Касир**
> Ні.

Далі діалог варіюється за знаннями гравця.

**Касир**
> Я два тижні дивлюсь на одну роботу. Все відкладав.

**Герой**
> І зараз саме час?

**Касир**
> Саме зараз я її вже не зроблю.

Передає контракт.

**Касир**
> Закрий її.

**Герой**
> А потім?

**Касир**
> А потім сам вирішиш.

Не проговорювати тезу “це теж твій фронт”.

## 15. Reusable mechanics worth implementing

### Field Receiver
High reuse. Recommended.

### Inspect / Record Point
Interaction that writes a structured field note:
- clean;
- shifted;
- damaged;
- blocked;
- unknown.

Reuse by Google, Orest, Kravets, procedural surveys.

### Evidence provenance
Quest/evidence item carries:
- source;
- sealed/opened;
- owner/context;
- discovered state.

### Sealed/Open container
Used by Semen, military, Protsent, evidence quests.

### Heavy mission cargo
Reuse current weight system.

### Story Raid Preset
World generator accepts hard constraints for authored expeditions.

### World-state props
Generator/lighting, shortages, 24.02 packing, closed routes.

Do not add yet:
- escort AI;
- carrying wounded;
- hacking minigame;
- disguise;
- persuasion stat;
- detective vision.

## 16. Contract grammar

Contract = **Archetype + Location + Complication + Optional Clause + Employer**

### Archetypes
1. Recovery
2. Cache
3. Survey
4. Route Check
5. Artifact Order
6. Salvage
7. Hunt (specific threat)
8. Investigation
9. Missing/Dead Retrieval
10. Delivery
11. State/Evidence Recovery
12. Signal Search

### Complications
- heavy cargo;
- radiation;
- anomaly field;
- known mutant den;
- possible hostile group;
- night/poor visibility;
- far extraction;
- uncertain location;
- sealed condition;
- inspect-before-retrieve.

### Optional clauses
- keep sealed;
- return extra evidence;
- do not remove object;
- return before condition/time window;
- recover specific side item;
- avoid damaging target equipment.

### Rewards
Not only money:
- грн;
- reputation;
- stock unlock;
- price discount;
- free repair;
- unique item;
- map intel;
- route unlock;
- new contact;
- contract tier.

## 17. Anti-patterns

Avoid:
- “2 fangs / 800 / +10 Loners” as narrative content;
- arbitrary kill counters;
- exact GPS for every investigation;
- “quest complete” after every interesting discovery;
- forcing every character into quest-giver role;
- generic “fetch scrap” without world reason;
- story information that exists only in dialogue and has no reusable world seed.
