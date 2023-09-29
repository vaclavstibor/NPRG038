# Weekly-Daily EMA 

## Uživatelská dokumentace
- Verze: 1.0
- Poslední aktualizace: 29.09.2023

## Úvod
Algoritmický přístup k analýze trhu. Tento projekt zahrnuje indikátory pro identifikaci trendu na libovolném grafu daného časovým rámcem (timeframe). Tyto indikátory jsou následně integrovány do samotné algoritmické logiky, která řídí správu obchodů.
Moving averages (MA) jsou rozděleny do třech rychlostních kategorií: Slow (Modrá), Medium (zelená) a Fast (červená). Stejné rozdělení platí i pro svíčky. Pro jednodušší pochopení daného algoritmického přístupu je vhodné shodně mapovat svíčkový a MA timeframe (tj. pokud Slow MA je 4H timeframe ze vstupu -> pak Slow candle je 4H svíčka). Následující popis je vedený způsobem: 
- Slow = 4H
- Medium = 1H
- Fast = 15M
- MAs metoda výpočtu: Exponencial (= MA označuji jako EMa)

Algorimus spouštějte vždy na Medium timeframu.

## Instrukce
- Program je navržený pro platformu cTrader. Abychom mohli algoritmus spustit, potřebujeme přístup k datům z trhu. Data nám poskytuje broker. V mém případě se jednalo o PurpleTrading, kde si musí každý uživatel založit účet s počátečním kapitálem, aby s ním mohl následně operovat.
- Projekt obsahuje jednotlivé indikátory a robota, jenž je třeba kompilovat prostřednictvím platformi cTrader do .algo spustitelného programu na platformě.  

- Přístup algoritmu dělíme na dvě části.
    - Indikátory
    - Robot
    ### Indikátory
    - Analytický nástroj navržený pro analýzu dat a jejich vizualizaci, který nemá přístup k správě obchodů.
        - `WeeklyDirection`
        - `DailyDirection`
        - `FastMA`
        - `MediumMA`
        - `SlowMA`
    ### Robot 
    - Obchodní algoritmus obstarávající veškerou logiku, jenž také řeší správu obchodů.
        - `Weekly-Daily-EMA`
## Algorithm
- [A Setup](#a-setup)
- [B1 Setup](#b1-setup)
- [B2 Setup](#b2-setup)
- [C Setup](#c-setup)
- [D Setup](#d-setup)

### A Setup 
#### LONG
1. **Weekly box**
    - Nakreslení modrého boxu pro nadcházející týden.
2. **Daily arrow**
    - Nakreslení modré šipky pro nadcházející den.
3. **Pullback k zelené (medium)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda:
        - Low svíčky je <= zelené EMě + threshold v pipech a zároveň Low svíčky je >= zelené EMě. 
        - Jsou EMy při dané svíčce správně seřazené (modrá < zelená < červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení zelené EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá nad červenou EMou.
    - (= simulování backu)
4. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je pozitivní nebo zavřela nad červenou EMou.
5. **Kontroluji seřazení aktuálních EM**
    - Modrá < Zelená < Červená

#### SHORT
1. **Weekly box**
    - Nakreslení červeného boxu pro nadcházející týden.
2. **Daily arrow**
    - Nakreslení červené šipky pro nadcházející den.
3. **Pullback k zelené (medium)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda-li:
        - High svíčky je >= zelené EMě - threshold a zároveň High svíčky je <= zelené EMě.
        - Jsou EMy při dané svíčce správně seřazené (modrá > zelená > červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení zelené EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá pod červenou EMou.
    - (= simulování backu)
4. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je negativní nebo zavřela pod červenou EMou.
5. **Kontroluji seřazení aktuálních EM**
    - Modrá > Zelená > Červená

---
### B1 Setup 
#### LONG
1. **Weekly box**
    - Nakreslení modrého boxu pro nadcházející týden.
2. **Pullback k zelené (medium)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda:
        - Low svíčky je <= zelené EMě + threshold v pipech a zároveň Low svíčky je >= zelené EMě. 
        - Jsou EMy při dané svíčce správně seřazené (modrá < zelená < červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení zelené EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá nad červenou EMou.
    - (= simulování backu)
3. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je negativní nebo zavřela pod červenou EMou.
4. **Kontroluji seřazení aktuálních EM**
    - Modrá < Zelená < Červená

#### SHORT
1. **Weekly box**
    - Nakreslení červeného boxu pro nadcházející týden.
2. **Pullback k zelené (medium)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda-li:
        - High svíčky je >= zelené EMě - threshold a zároveň High svíčky je <= zelené EMě.
        - Jsou EMy při dané svíčce správně seřazené (modrá > zelená > červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení zelené EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá pod červenou EMou.
    - (= simulování backu)
3. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je negativní nebo zavřela pod červenou EMou.
4. **Kontroluji seřazení aktuálních EM**
    - Modrá > Zelená > Červená

---
### B2 Setup 
#### LONG
1. **Weekly box**
    - Nakreslení modrého boxu pro nadcházející týden.
2. **Daily arrow**
    - Nakreslení modré šipky pro nadcházející den.
3. **Pullback k modré (slow)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda:
        - Low svíčky je <= modré EMě + treshold v pipech a zároveň Low svíčky je >= modré EMě. 
        - Jsou EMy při dané svíčce správně seřazené (modrá < zelená < červená) a zároveň je rozdíl mezi zelenou a modrou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení modré EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá nad červenou EMou.
    - (= simulování backu)  
4. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je pozitivní nebo zavřela nad červenou EMou.
5. **Kontroluji seřazení akruálních EM**
    - Modrá < Zelená < Červená

#### SHORT
1. **Weekly box**
    - Nakreslení červeného boxu pro nadcházející týden.
2. **Daily arrow**
    - Nakreslení červené šipky pro nadcházající den.
3. **Pullback k modré (slow)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda-li:
        - High svíčky je >= modré EMě - threshold a zároveň High svíčky je <= modré EMě.
        - Jsou EMy při dané svíčce správně seřazené (modrá > zelená > červená) a zároveň je rozdíl mezi zelenou a modrou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení zelené EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá pod červenou EMou.
    - (= simulování backu)
4. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je negativní nebo zavřela pod červenou EMou.
5. **Kontroluji seřazení aktuálních EM**
    - Modrá > Zelená > Červená

---
### C Setup 
#### LONG
1. **Pullback k červené (fast)**
    - Dívám se jednu svíčku dozadu a kontroluji, zda:
        - Low svíčky je <= červené EMě + treshold v pipech a zároveň Low svíčky je >= červené EMě.
        - Jsou EMy při dané svíčce správně seřazené (modrá < zelená < červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrashould (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení červené EMy (podle výše uvedeného), dívám se zda 1. (stejná) svíčka od aktuální zavírá nad červenou EMou.
    - (= simulování backu)
2. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je pozitivní nebo zavřela nad červenou EMou.
3. **Kontroluji seřazení akruálních EM**
    - Modrá < Zelená < Červená

#### SHORT
1. **Pullback k červené (fast)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda-li:
        - High svíčky je >= červené EMě - threshold a zároveň High svíčky je <= červené EMě.
        - Jsou EMy při dané svíčce správně seřazené (modrá > zelená > červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení červené EMy (podle výše uvedeného), dívám se zda 1. svíčka od aktuální zavírá pod červenou EMou.
    - (= simulování backu)
2. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je negativní nebo zavřela pod červenou EMou.
3. **Kontroluji seřazení aktuálních EM**
    - Modrá > Zelená > Červená

### D Setup 
#### LONG
1. **Weekly box**
    - Nakreslení červeného boxu pro nadcházející týden.
2. **Daily arrow**
    - Nakreslení červené šipky pro nadcházející den.
3. **Pullback k zelené (medium)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda:
        - Low svíčky je <= červené EMě + threshold v pipech a zároveň Low svíčky je >= červené EMě. 
        - Jsou EMy při dané svíčce správně seřazené (modrá < zelená < červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení červené EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá nad červenou EMou.
    - (= simulování backu)
4. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je pozitivní nebo zavřela nad červenou EMou.
5. **Kontroluji seřazení aktuálních EM**
    - Modrá < Zelená < Červená

#### SHORT
1. **Weekly box**
    - Nakreslení modrého boxu pro nadcházející týden.
2. **Daily arrow**
    - Nakreslení modrého šipky pro nadcházející den.
3. **Pullback k zelené (medium)**
    - Procházím 9 svíček dozadu a u každé kontroluji, zda-li:
        - High svíčky je >= červené EMě - threshold a zároveň High svíčky je <= červené EMě.
        - Jsou EMy při dané svíčce správně seřazené (modrá > zelená > červená) a zároveň je rozdíl mezi zelenou a červenou EMou >= thrasholdu (= aby nedocházelo k poplašnému pullbacku při křížení EM, nasimuluji rozdílem MAs vzdálenost).
        - (= simulování pull)
    - Pokud došlo k prolomení červené EMy (podle výše uvedeného) jednou z 9 svíček, dívám se zda 1. svíčka od aktuální zavírá pod červenou EMou.
    - (= simulování backu)
4. **Kontrola 4H svíčky (slow)**
    - Dívám se zda 1. svíčka od aktuální je negativní nebo zavřela pod červenou EMou.
5. **Kontroluji seřazení aktuálních EM**
    - Modrá > Zelená > Červená

## Input parameters
- **Setups**
    - Allow [A setup](#a-setup)
    - Allow [B1 setup](#b1-setup)
    - Allow [B2 setup](#b2-setup)
    - Allow [C setup](#c-setup)
    - Allow [D setup](#d-setup)
- **Candles Timeframes**
    - Fast Timeframe
    - Medium Timeframe
    - Slow Timeframe    
- **Money Management**
    - Initial Quantity (Lots)
        - *Lots size for all positions.*
    - Stop Loss
    - Tak Profit
    - Number of orders
        - *Async orders in one time.*   
- **PullBacks**
    - Fast MA PullBack ThresHold
        - *ThresHold in pips*
    - Medium MA PullBack ThresHold
        - *ThresHold in pips*
    - Slow MA PullBack ThresHold
        - *ThresHold in pips*
- **Partial Close**
    - Allow Positions partial close
    - Distance for first partial close
    - Size of first partial close
    - Distance for second partial close
    - Size of second partial close
- **Trailing Stop**
    - Allow Positions trailing stop
    - Trail point
        - *Distance when trail occurate*
    - *pouze za předpokladu 2:1 RRR*
- **Trading Hours**
    - Trading hour start
    - Trading hour stop
    - *v nynější verzi není podporováno*  
- **RSI**
    - Source
    - Period
    - *v nynější verzi není podporováno*
- **MA**
    - Fast EMA Periods
    - Medium EMA Periods
    - Slow EMA Periods    

## Notes
- Pravidla pro dodržení jsou striktně daná. Algo si všimne veškerých příležitostí, a někdy především i těch, které lidským okem přehlédneme a možná někdy i nechceme.

