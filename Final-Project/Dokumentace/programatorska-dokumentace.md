# Weekly Daily EMA
## Programátorská Dokumentace
- Verze: 1.0
- Poslední aktualizace: 29.09.2023

### Úvod:
Algoritmický přístup k analýze trhu. Tento projekt zahrnuje indikátory pro identifikaci trendu na libovolném grafu daného časovým rámcem (timeframe). Tyto indikátory jsou následně integrovány do samotné algoritmické logiky, která řídí správu obchodů.
Moving averages (MA) jsou rozděleny do třech rychlostních kategorií: Slow (Modrá), Medium (zelená) a Fast (červená). Stejné rozdělení platí i pro svíčky. Pro jednodušší pochopení daného algoritmického přístupu je vhodné shodně mapovat svíčkový a MA timeframe (tj. pokud Slow MA je 4H timeframe ze vstupu -> pak Slow candle je 4H svíčka). Následující popis je vedený způsobem: 
- Slow = 4H
- Medium = 1H
- Fast = 15M
- MAs metoda výpočtu: Exponencial (= MA označuji jako EMa)

Algorimus spouštějte vždy na Medium timeframu.

### Použití:
Program se spouští prostřednictvím platformy cTrader s daty brokera, jenž platforma podporuje.

### Weekly-Daily-EMA.cs (Robot)
___
### Struktura Candles:
Představuje kolekci svíčkových dat.
Obsahuje svíčková data pro časové rámce rychlého (fast), středního (medium), pomalého (slow), denního (D1), týdenního (W1) a aktuálního časového rámce.
Každý časový rámec má vlastnost (property), která uchovává svíčková data ve struktuře Candle.
___

### Struktura Candle:
Představuje jednu svíčku, která je základní jednotkou používanou ve finanční analýze.
Obsahuje různé atributy svíčky, jako jsou cena, otevření, uzávěr, minimální a maximální cena, střední hodnota a další.
Obsahuje také vlastnosti pro určení směru svíčky, kontrolu, zda je svíčka Doji, a další.
___

### Enum EDirection:

Reprezentuje směr svíčky (negativní nebo pozitivní) na základě uzavírací ceny svíčky vzhledem k tělu svíčky.
___

### Třída MovingAverages:
Statická třída, která uchovává pole pro klouzavé průměry (různé časové rámce) používané ve finanční analýze.
___

### Třída InputParams:
Statická třída, která uchovává různé vstupní parametry používané v analýze.
___

### Vstupní parametry:
V kódu každý region obsahuje parametry související s konkrétní oblastí obchodování, jako jsou povolení různých obchodních setupů, časové rámce pro analýzu, správu kapitálu, pullbacky, částečné uzávěry pozic, trailing stopy, obchodní hodiny a další indikátory.
Každý parametr má název, skupinu (group), výchozí hodnotu (default value) a další specifikace. Detailnější popis je uveden přímo v kódu.
___

### Metoda OnStart():
Tato metoda je volána při spuštění obchodního robota a má za úkol provádět inicializaci různých komponent a parametrů potřebných pro obchodování.

Na začátku metody se nachází podmínka #if DEBUG, která umožňuje spustit debugger, pokud je definována preprocesorová direktiva DEBUG. To je užitečné pro ladění kódu ve Visual studio.

Následuje inicializace Exponential Moving Average (EMA) indikátorů pro různé časové rámce:

- `FastEMA` je inicializován pomocí metody `Indicators.GetIndicator<FastEMA>(Bars.ClosePrices, FastMAPeriod)`. Tímto způsobem získáváme rychlý EMA indikátor.
- `MediumEMA` je inicializován podobným způsobem, ale pro střední EMA.
- `SlowEMA` je inicializován pro pomalý EMA.
Následně jsou hodnoty posledních 10 EMA hodnot pro různé časové rámce uloženy do statických polí ve třídě `MovingAverages`. Tyto hodnoty jsou získány z EMA indikátorů.

Metoda Print je použita k výpisu prvotních hodnot rychlého, středního a pomalého průměru na konzoli. 

Dále dochází k inicializaci objektů tříd A, B1, B2, C, a D:

Tyto objekty představují jednotlivé obchodní strategie nebo setupy a jsou inicializovány s instancí samotného robota (`this`). Tyto objekty budou použity pro provádění konkrétních obchodních strategií na základě nastavených parametrů.
Tímto způsobem je metoda `OnStart` připravena k inicializaci všech potřebných komponent a parametrů.
___

### Metoda OnTick()
Jolána pro každý tick (změnu ceny) a zajišťuje různé úkoly související s monitorováním a aktualizací dat svíček, nastavením obchodních strategií a prováděním částečných uzávěrů pozic.

1. Kontrola počtu svíček pro různé časové rámce.Tato část kódu provádí kontrolu, zda došlo ke změně počtu svíček v různých časových rámcích.

    Pro každý z časových rámců (`FastTimeframe`, `MediumTimeframe`, `SlowTimeframe`, `TimeFrame.Daily`, `TimeFrame.Weekly` a aktulání `TimeFrame`, na kterém je robot spuštěný) se provádí následující postup.

    Pokud se počet svíček v daném časovém rámci změnil (`FastBarsCount != MarketData.GetBars(FastTimeframe, Symbol.Name).Count`), vytvoří se nový objekt `FastCandle` (pro časový rámec "Fast") nebo `MediumCandle` (pro časový rámec "Medium") atd. s použitím nejnovějších dat z odpovídajícího časového rámce.

    Aktualizuje se proměnná `FastBarsCount` (nebo `MediumBarsCount`, `SlowBarsCount`, `D1BarsCount`, `W1BarsCount`, `CurrentBarsCount`) tak, aby odpovídala aktuálnímu počtu svíček v daném časovém rámci.

    Po provedení kontroly počtu svíček pro všechny časové rámce se svíčky zaznamenají do Candles. 
    
2. Provádění obchodních strategií:

    Jednotlivé obchodní setupy strategie (A, B1, B2, C, D) se provádějí na základě aktuálních dat svíček a vstupních parametrů pro povolení jednotlivých strategií (`ExtAllowASetup`, `ExtAllowB1Setup`, atd.).

    Pokud strategie vrací true z volání její metody Setup (což indikuje, že byly splněny obchodní podmínky pro tuto strategii), poté v závislosti na typu pozice (`TradeType.Sell` nebo `TradeType.Buy`) se provádí příslušná akce:
    
    - Pokud je typ pozice `Sell`, spustí se metoda `SetSellPositionAsync()` pro otevření prodejní pozice, a počet pozic pro danou strategii se zvýší o hodnotu `ExtAmountOfOrders`.
    - Pokud je typ pozice `Buy`, spustí se metoda `SetBuyPositionAsync()` pro otevření nákupní pozice, a počet pozic pro danou strategii se zvýší o hodnotu `ExtAmountOfOrders`.

3. Nakonec se provádí metody `ExecutePartialClose()` a `ExecuteTrailingStop()`, které zajistí částečné uzavírání pozic, pokud je tato funkce povolena na vstupu, a stejně tak trailing stop.
___

### Rozhraní ISetup
Rozhraní ISetup definuje smlouvu (kontrakt) pro nastavení a vyhodnocení podmínek pro otevření prodejních a nákupních pozic na základě historických dat svíček. Pomocí tohoto rozhraní lze přizpůsobit nastavení a podmínky pro konkrétní obchodní strategii.

#### Metoda Setup
Metoda Setup je zodpovědná za inicializaci jakýchkoli potřebných parametrů nebo nastavení pro obchodní strategii. Přijímá objekt `Candles`, který reprezentuje historická data svíček, a `bool`, který označuje, zda je obchod povolen nebo ne. Metoda vrací `true`, pokud byly splněny podmínky pro otevření pozice, jinak vrací `false`.

#### Metoda OpenSellPositionConditions
Tato metoda slouží k vyhodnocení podmínek pro otevření prodejní pozice na základě historických dat svíček. Vrací `true`, pokud jsou splněny podmínky pro otevření prodejní pozice, jinak vrací `false`.

#### Metoda OpenBuyPositionConditions
Tato metoda slouží k vyhodnocení podmínek pro otevření nákupní pozice na základě historických dat svíček. Vrací `true`, pokud jsou splněny podmínky pro otevření nákupní pozice, jinak vrací `false`.

___

### Třída A
Třída `A` představuje konkrétní obchodní strategii, která může být použita v rámci obchodního algoritmu `Algo`. Tato třída definuje podmínky a akce pro otevření pozic na základě historických dat svíček a konfigurací nastavení.

#### Vlastnosti třídy
`Algo`: Privátní vlastnost, která uchovává odkaz na rodičovský obchodní algoritmus `Algo`.
`PositionType`: Vlastnost, která určuje typ pozice (prodejní nebo nákupní).
`PositionsCounter`: Vlastnost, která udržuje počet pozic otevřených touto strategií pro pozdější statistiku.

#### Konstruktor třídy
Konstruktor třídy `A` přijímá odkaz na rodičovský obchodní algoritmus `Algo` a inicializuje vlastnosti třídy, včetně `Algo`, `PositionType` a `PositionsCounter`.

##### Metoda Setup
Metoda `Setup` definuje nastavení podmínek pro obchodní strategii `A` a provádí nastavení, pokud je povoleno. Tato metoda přijímá `Candles`, což je kolekce historických dat svíček, a `isAllowed`, což je vlajka indikující, zda je obchod povolen. `Metoda` vyhodnocuje podmínky na základě směru svíček (`Weekly` a `Daily`) a pokud jsou splněny podmínky pro otevření prodejní nebo nákupní pozice, vrací `true`. Tato metoda také nastavuje `PositionType` na odpovídající typ pozice.

#### Metody OpenSellPositionConditions a OpenBuyPositionConditions:
Tyto metody definují podmínky pro otevření prodejních a nákupních pozic na základě historických dat svíček. Metoda `OpenSellPositionConditions` vyhodnocuje podmínky pro prodejní pozici, zatímco metoda `OpenBuyPositionConditions` vyhodnocuje podmínky pro nákupní pozici. Metody vrací `true`, pokud jsou splněny podmínky pro otevření pozice, jinak vrací `false`.
___

### Třídy B1, B2, C, D
Stejně jako pro třídu `A`, avšak se liší logikou, která je popsáná v (komentářích) kódu.
___

### Třída PullBack
Reprezentuje třídu pro vyhodnocení podmínek pullbacku na základě hodnot klouzavých průměrů a historických dat svíček.

obsahuje privátní vlastnost `Algo`, která uchovává odkaz na rodičovský obchodní algoritmus `Algo`.

#### Metoda FastMA
Metoda `FastMA` vyhodnocuje podmínky pro pullback založený na rychlém klouzavém průměru pro určitý typ obchodu. Metoda přijímá parametr `tradeType`, který určuje daný typ obchodu. Vrací `true`, pokud jsou splněny podmínky pro pullback, jinak vrací `false`. Metoda zkoumá, zda cena svíčky dosáhla hodnoty rychlého klouzavého průměru s určeným prahem (`threshold`). Tato podmínka je kontrolována pro několik minulých svíček (konkrétně 9).

#### Metoda MediumMA
Metoda `MediumMA` vyhodnocuje podmínky pro pullback založený na středním klouzavém průměru pro určitý typ obchodu. Metoda přijímá parametr `tradeType`, který určuje typ obchodu. Vrací `true`, pokud jsou splněny podmínky pro pullback, jinak vrací `false`. Metoda zkoumá, zda cena svíčky dosáhla hodnoty středního klouzavého průměru s určeným prahem (`threshold`). Tato podmínka je kontrolována pro několik minulých svíček (konkrétně 9).

#### Metoda SlowMA
Metoda `SlowMA` vyhodnocuje podmínky pro pullback založený na pomalém kouzavém průměru pro určitý typ obchodu. Metoda přijímá parametr `tradeType`, který určuje typ obchodu. Vrací `true`, pokud jsou splněny podmínky pro pullback, jinak vrací `false`. Metoda zkoumá, zda cena svíčky dosáhla hodnoty pomalého klouzavého průměru s určeným prahem (`threshold`). Tato podmínka je kontrolována pro několik minulých svíček (konkrétně 9).
___

### WeeklyDirection.cs (Indicator)
Tento indikátor umožňuje sledovat směr týdenní svíčky a vizualizovat týdenní rozsah na grafu.

#### Třída WeeklyDirection
Reprezentuje indikátor, který zobrazuje směr týdenní svíčky a obdélník na grafu, jenž představuje týdenní rozsah obchodování.

#### Metoda Initialize 
Je volána při inicializaci indikátoru, ale neobsahuje žádnou specifickou inicializační logiku.

#### Metoda Calculate
Metoda Calculate je povinná metoda pro výpočet hodnot indikátoru a aktualizaci vizuálního zobrazení na grafu. Tato metoda provádí následující akce:
- Získá historická data pro týdenní a aktuální svíčky.
- Kontroluje, zda došlo ke změně počtu týdenních svíček, což indikuje novou týdenní svíčku.
- Vytvoří objekt `WeeklyCandle` reprezentující nejnovější týdenní svíčku.
- Na základě směru týdenní svíčky určí barvu pro vizualizaci na grafu.
- Kreslí obdélník na grafu, který reprezentuje týdenní rozsah obchodování.
- Provádí další výpočty a aktualizace indikátoru podle potřeby.
___

### DailyDirection.cs (Indicator)
Tento indikátor zobrazuje směr denních svíček na grafu a používá šipky nahoru a dolů k vizualizaci tohoto směru.

#### Třída DailyDirection
Reprezentuje indikátor, který zobrazuje směr denních svíček šipkami nahoru a dolů na grafu.

#### Metoda Initialize 
Je volána při inicializaci indikátoru, ale neobsahuje žádnou specifickou inicializační logiku.

#### Metoda Calculate
Je povinná metoda pro výpočet hodnot indikátoru a zobrazení směru denních svíček na grafu. Tato metoda provádí následující akce:
- Získá historická data pro denní a aktuální svíčky.
- Kontroluje, zda došlo ke změně počtu denních svíček, což indikuje novou denní svíčku.
- Vytvoří objekt `DailyCandle` reprezentující nejnovější denní svíčku.
- Na základě směru denní svíčky vykreslí šipku nahoru nebo dolů na grafu.
- Aktualizuje počet denních svíček.
- Výpočty a aktualizace indikátoru mohou být prováděny dle potřeby.
___

### Fast/Medium/SlowMA.cs (Indicator)
Slouží k výpočtu Exponenciálního klouzavého průměru (EMA) určité datové řady na základě konkrétního časového rámce.

#### Source
Datová řada, ze které bude počítán EMA.
#### Periods
Počet období (= počet svíček na daném timeframu), který bude použit pro výpočet EMA.

#### Result
Výstupem je indikátorová datová řada, která obsahuje vypočítaný EMA.

#### Privátní pole a proměnná
- `exp`: Proměnná pro výpočet exponenciálního faktoru.
- `last10EMAValues`: Pole pro ukládání posledních 10 hodnot EMA pro účely analýzy.

#### Metoda Initialize
Volá se při inicializaci indikátoru. V této metodě se provádí následující kroky:
- Výpočet exponenciálního faktoru na základě zadaného periody.
- Inicializace pole `last10EMAValues` pro ukládání posledních 10 hodnot EMA.

#### Metoda Calculate
Metoda Calculate je povinná metoda, která provádí výpočet EMA na základě zadané datové řady a periody. Tato metoda provádí následující kroky:
- Získání předchozí hodnoty EMA.
- Kontrola, zda je předchozí hodnota `NaN` (není číslo).
- Pokud je předchozí hodnota `NaN`, použije se hodnota z datové řady Source jako počáteční hodnota EMA.
- Výpočet EMA pomocí exponenciálního vyhlazovacího vzorce.
___
