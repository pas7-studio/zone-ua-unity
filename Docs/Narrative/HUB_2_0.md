# Zone-UA — Hub 2.0 Narrative / Spatial Baseline

Статус: implemented concept, further polish allowed  
Purpose: зафіксувати просторові й постановочні обмеження перед NPC schedule та сюжетними state overrides.

## 1. Core fantasy

Хаб — не “safe-zone menu”. Це стара господарська база/лісниця, яку роками пристосували до життя в Зоні.

Гравець після рейду має відчувати: **“я повернувся”**, а не “я в сервісній зоні”.

## 2. Protected intro reveal

**HARD LOCK.**

Поточний hero moment не ламати:

forest trail  
→ fog  
→ hub streaming/loading  
→ control lock  
→ Гугл і герой самі доходять  
→ smooth zoom out  
→ short time pass  
→ music  
→ ZONE-UA title.

Перед будь-якою суттєвою зміною геометрії:
- запускати актуальний пролог;
- перевіряти sightline;
- не ставити нові масивні об'єкти в foreground;
- не змінювати intro path без окремого рішення.

Hub 2.0 росте **від цього кадру**, а не навпаки.

## 3. Perimeter

Немає суцільної фортеці.

Старий паркан:
- місцями цілий;
- місцями просів;
- місцями з дірами;
- місцями відсутній;
- місцями його замінюють будівлі/ліс.

Нижня сторона хаба широко відкрита до дороги.

Навколо хаба можна пройти пішки. Не використовувати invisible wall як базову межу.

## 4. Spatial layers

### Warm core
Дідо, вогонь, central yard, житло.

### Working belt
Семен, Шуруп, генератор, Орест, медицина, логістика.

### Wild edge
дірявий паркан, дровник, криниця, технічні стежки, ліс, raid trail.

## 5. Road & trails

- стара широка ґрунтова service road проходить по краю/нижній частині хаба;
- intro trail — окремий пішохідний маршрут через ліс і туман;
- raid trail — окрема менша стежка від дороги/краю хаба;
- raid transition не стоїть у паркані як “portal”: гравець фізично проходить 15–30 секунд у ліс, дерева стискаються, туман маскує transition;
- повернення працює навпаки;
- після першого cinematic reveal звичайні повернення не забирають control.

## 6. Functional areas

### Central yard
Відкрита неправильна витоптана площа. Event space. Без постійного NPC в центрі.

### Dido canopy
Соціальне серце. 6–8 місць для різних поз/активностей. Радіо, вогонь, кухня.

### Housing
Маленький напівприватний двір. Ліжка, stash героя, особисті речі інших мешканців.

### Well / water
Окремий мікропростір за житлом. Майбутній activity anchor.

### Shurup garage
Реальний гараж/майстерня, а не kiosk. Верстак, техніка, salvage.

### Generator corner
Фізичне джерело електрики хаба. Кабелі в різні зони. Майбутні blackout/story state.

### Semen warehouse
Ближче до дороги/логістики. Видимий stock і back storage.

### Orest lab
Трохи осторонь, ближче до дикого краю. Sensor/antenna path у ліс.

### Medical point
Між житлом і центральним простором. Зарезервований під лікування/поранених.

### Kravets / raid control
Малий пост біля raid trail. Не окрема військова база.

### Outer ring
Walkable, не декоративний. Курилка, дровник, сміття, тихий кут, технічні точки.

## 7. Landmarks

1. Dido fire.
2. Orest antenna.
3. Shurup garage.
4. Kravets/raid light or generator sound.

Гравець має орієнтуватися без minimap.

## 8. Schedule readiness

На цьому етапі не використовувати random wandering як головну поведінку.

Кожна зона повинна мати майбутні semantic anchors:
- work;
- eat;
- rest;
- talk;
- guard;
- radio;
- sleep;
- smoke;
- inspect;
- carry/load.

Routine system буде окремим етапом поверх уже зібраного хаба.

## 9. 24.02 readiness

Та сама геометрія повинна підтримати:
- натовп біля радіо Діда;
- Кравця на зв'язку;
- пакування Касира;
- відкриті військові ящики;
- машину/виїзд через логістичну сторону;
- закритий/контрольований raid route;
- проліт пошкодженого БПЛА над краєм хаба;
- повернення гравця з crash-site raid у соціально змінений хаб.

Не будувати окрему “24.02 version” рівня. Використовувати state overrides, props, NPC schedules, dialogue/ambient changes.
