# ♟ ChessPlatform

Eine Schachplattform im Browser, die ich Schritt für Schritt mit **Blazor (.NET)** gebaut habe. Man kann eine vollständige Partie nach den echten Schachregeln spielen, sich registrieren und anmelden, und die eigenen Partien werden in einer Datenbank gespeichert und lassen sich später Zug für Zug wieder ansehen.

> Dieses Projekt ist auch ein Lernprojekt. Ich wollte nicht nur etwas haben, das läuft, sondern verstehen, **warum** es läuft. Unten im Abschnitt „Wie ich mit der KI gearbeitet habe“ steht genau, wie ich dabei vorgegangen bin.

---

## Inhaltsverzeichnis

1. [Was kann das Projekt?](#was-kann-das-projekt)
2. [Screenshots](#screenshots)
3. [Technik im Überblick](#technik-im-überblick)
4. [Projektstruktur](#projektstruktur)
5. [So habe ich es gebaut (Schritt für Schritt)](#so-habe-ich-es-gebaut-schritt-für-schritt)
6. [Was war einfach, was war kompliziert?](#was-war-einfach-was-war-kompliziert)
7. [Wie ich mit der KI gearbeitet habe](#wie-ich-mit-der-ki-gearbeitet-habe)
8. [Projekt starten](#projekt-starten)
9. [Aktueller Stand](#aktueller-stand)
10. [Wie es weitergehen soll](#wie-es-weitergehen-soll)

---

## Was kann das Projekt?

**Das Spiel**
- Schachbrett mit Startaufstellung, Koordinaten (a–h, 1–8) und Figuren
- Figur anklicken: mögliche Ziele werden markiert (Punkt bei leeren Feldern, roter Ring bei schlagbaren Figuren)
- Alle Figuren ziehen nach den echten Regeln
- Man kann sich nicht selbst ins Schach stellen, gefesselte Figuren bleiben gefesselt
- Schach (König wird rot markiert), Schachmatt und Patt
- Sonderzüge: **Rochade**, **en passant** und **Bauernumwandlung** (mit Auswahldialog)
- Remisregeln: dreifache Stellungswiederholung, 50-Züge-Regel, zu wenig Material
- Zugliste in echter Schachnotation (zum Beispiel `e4`, `Nf3`, `Bxc6`, `O-O`, `Qh4#`)
- Anzeige der geschlagenen Figuren
- Aufgeben (mit Nachfrage) und Remis anbieten / annehmen / ablehnen
- Brett drehen und neue Partie starten

**Die Plattform**
- Registrieren, Anmelden und Abmelden
- Angemeldete Benutzer: Die Partie wird nach jedem Zug automatisch in der Datenbank gespeichert
- Seite **„Meine Partien“** mit Datum, Anzahl der Züge, Ergebnis und Ausgang
- **Partie ansehen:** Ich kann mit Anfang / Zurück / Vor / Ende durch eine gespeicherte Partie gehen

---

## Screenshots

> Hier füge ich meine eigenen Screenshots ein. Dateien am besten im Ordner `docs/screenshots/` ablegen und so einbinden:
> `![Beschreibung](docs/screenshots/dateiname.png)`

### Das Schachbrett in der Startaufstellung
<img width="1216" height="774" alt="image" src="https://github.com/user-attachments/assets/a0af33f9-3564-4309-bbaf-5a4c2d2a440a" />

### Mögliche Züge und Auswahl einer Figur
<img width="1160" height="694" alt="image" src="https://github.com/user-attachments/assets/c63ed5bc-21f9-45fa-9ca0-062940cc5243" />

### Schach, Schachmatt und Statusanzeige
<img width="1225" height="795" alt="image" src="https://github.com/user-attachments/assets/a561b7f3-43ab-4a76-af89-a42719efa404" />

### Umwandlungs-Dialog
<img width="1153" height="692" alt="image" src="https://github.com/user-attachments/assets/a469c924-f66f-41f7-b3ea-8cd0754d76f2" />

### Zugliste, geschlagene Figuren, Aufgeben und Remis
<img width="396" height="560" alt="image" src="https://github.com/user-attachments/assets/64dc5896-d0fb-4a43-b02b-b3c476ff220d" />

### Registrierung und Anmeldung
<img width="1304" height="516" alt="image" src="https://github.com/user-attachments/assets/97320a6e-504e-42ed-bbdd-d709dc3a6555" />
<img width="680" height="415" alt="image" src="https://github.com/user-attachments/assets/e88b4aa6-f8f2-409c-8e3b-15c5de2c2a4e" />

### Meine Partien
<img width="1036" height="406" alt="image" src="https://github.com/user-attachments/assets/2d084e20-4f68-4bc4-ab35-36cdf838b99b" />


### Partie ansehen (Züge durchklicken)
<img width="1042" height="765" alt="image" src="https://github.com/user-attachments/assets/44f0d70f-e5b5-460f-aa21-5abe5f2adf14" />
<img width="1044" height="771" alt="image" src="https://github.com/user-attachments/assets/8eaa4039-8c47-4e3e-abc0-a94f8ba2e479" />
<img width="1076" height="756" alt="image" src="https://github.com/user-attachments/assets/846a24b8-5a83-4bf5-ae97-9f293b6cc172" />

### Die Datenbank in SQL Server Management Studio
<!-- ![Datenbank](docs/screenshots/datenbank.png) -->
*Screenshot folgt*

---

## Technik im Überblick

| Bereich | Was ich benutzt habe | Wofür |
|---|---|---|
| Framework | **Blazor Web App** (.NET, Interactive Server) | Oberfläche und Logik in C#, ohne eigenes JavaScript |
| Styling | **Tailwind CSS** (über npm und die Tailwind-CLI) | Schnelles Gestalten direkt im HTML |
| Benutzer | **ASP.NET Core Identity** | Registrierung, Login, Passwörter, Konto |
| Datenbank | **SQL Server LocalDB** | Speichern der Benutzer und Partien |
| Datenzugriff | **Entity Framework Core** mit Migrationen | Tabellen als C#-Klassen, Datenbank wird daraus erzeugt |
| Datenbank ansehen | **SQL Server Management Studio (SSMS)** | Kontrolle, ob wirklich etwas in der Datenbank steht |
| Versionsverwaltung | **Git** | Viele kleine Commits, ein Schritt pro Commit |

---

## Projektstruktur

```
Chess/
├── Components/
│   ├── Layout/          MainLayout (Grundlayout), NavMenu (Navigation)
│   ├── Pages/           Home, Play (Spielen), Games (Meine Partien), GameView (Partie ansehen)
│   └── Game/            ChessBoard, ChessSquare, ChessPiece, MoveList, CapturedPieces
├── Models/              Die Schachlogik, unabhängig von Oberfläche und Datenbank
│   ├── Board.cs           Das Brett (8x8 Felder)
│   ├── Piece.cs           Figur = Typ + Farbe
│   ├── Square.cs          Ein Feld (Reihe, Linie)
│   ├── MoveGenerator.cs   Zugregeln der einzelnen Figuren
│   ├── AttackChecker.cs   Wird ein Feld angegriffen? (für Schach)
│   ├── Game.cs            Spielzustand, legale Züge, Sonderzüge, Remisregeln, Notation
│   ├── GameStatus.cs      Laufend, Schach, Matt, Patt, Remis-Arten, Aufgabe
│   └── MoveRecord.cs      Ein gespielter Zug mit Notation
├── Data/                Alles rund um die Datenbank
│   ├── ApplicationDbContext.cs
│   ├── GameEntities.cs    StoredGame und StoredMove (die Tabellen)
│   ├── GameRepository.cs  Speichern und Laden von Partien
│   └── Migrations/
└── Styles/app.css       Tailwind-Eingabedatei mit meinen Brettfarben
```

**Wichtigste Idee dahinter:** Die Schachlogik im Ordner `Models` weiß nichts von Blazor und nichts von der Datenbank. Sie ist reines C#. Die Oberfläche fragt nur „welche Züge sind erlaubt?“ und „führe diesen Zug aus“. Dadurch konnte ich später die Datenbank dazubauen, ohne an den Schachregeln etwas zu ändern.

---

## So habe ich es gebaut (Schritt für Schritt)

Ich habe das Projekt bewusst in kleine Schritte geteilt und nach jedem Schritt getestet und einen Git-Commit gemacht. So konnte ich Fehler immer auf den letzten Schritt eingrenzen.

### Schritt 0: Grundgerüst und Tailwind
Ich habe ein Blazor-Projekt mit Benutzerkonten erstellt. Die Vorlage bringt Bootstrap mit, das habe ich durch Tailwind CSS ersetzt (Installation über npm, ein Build-Skript, das aus meiner `app.css` die fertige `tailwind.css` erzeugt). Dort habe ich auch die Brettfarben als eigene Farben definiert.

### Schritt 1: Brett und Figuren anzeigen
Ein `Board` ist ein 8x8-Array aus Figuren. Jedes Feld ist eine kleine Komponente (`ChessSquare`), das ganze Brett besteht aus 64 davon in einem Raster. Die Figuren sind Unicode-Zeichen (♔ ♕ ♖ …), weiße mit Schatten, damit man sie auf hellen Feldern sieht. Dazu kamen Koordinaten und „Brett drehen“.

### Schritt 2: Figuren ziehen
Jede Figur hat ihre Regeln im `MoveGenerator`. Es gibt nur drei Grundmuster, die sich wiederverwenden lassen:
- **Einzelschritte** (Springer, König)
- **Gleiten** in eine Richtung bis zum Rand oder zu einer Figur (Läufer, Turm, Dame)
- **Bauern** als Sonderfall (vorwärts ziehen, diagonal schlagen, Doppelschritt am Anfang)

Danach kam die Klicksteuerung: Figur wählen, Ziele anzeigen, Ziel klicken, ziehen, die andere Farbe ist dran.

### Schritt 3: Schach, Matt und Patt
Ein Zug ist nur erlaubt, wenn der eigene König danach nicht angegriffen wird. Dafür probiert das Spiel jeden möglichen Zug auf einer **Kopie des Bretts** aus und schaut, ob der König danach im Schach steht. Der `AttackChecker` prüft dafür, ob ein Feld von einer Farbe angegriffen wird. Hat ein Spieler keinen legalen Zug mehr, ist es Matt (mit Schach) oder Patt (ohne Schach).

### Schritt 4: Sonderzüge
Rochade, en passant und Umwandlung. Alle drei laufen über **eine gemeinsame Methode `ApplyMove`**, die sowohl für echte Züge als auch für die Schach-Prüfung auf der Kopie benutzt wird. Dadurch werden diese Sonderzüge automatisch richtig auf Selbstschach geprüft.

### Schritt 5: Remisregeln
Dreifache Stellungswiederholung, 50-Züge-Regel und zu wenig Material. Das Spiel merkt sich dafür, wie oft jede Stellung vorkam.

### Schritt 6: Komfort
Zugliste in Notation, geschlagene Figuren, Aufgeben und Remis anbieten. Jeder Zug wird als `MoveRecord` gespeichert, das war schon die Vorbereitung für die Datenbank.

### Schritt 7: Benutzer und Datenbank
Datenbank per Migration anlegen, Login und Registrierung in die Navigation einbauen, dann zwei eigene Tabellen (`Games` und `Moves`), Speichern nach jedem Zug, die Seite „Meine Partien“ und die Ansicht zum Durchklicken.

---

## Was war einfach, was war kompliziert?

### Eher einfach
- **Brett und Figuren anzeigen.** Ein Raster, 64 Felder, abwechselnd hell und dunkel.
- **Die Zugregeln von Springer, König, Turm, Läufer und Dame.** Sobald die Muster „Einzelschritt“ und „Gleiten“ klar waren, war der Rest nur Wiederverwendung.
- **Tailwind einrichten.** Ein paar Befehle, danach schreibt man die Klassen direkt in die Komponenten.
- **Die Datenbank anlegen.** Mit Entity Framework reicht ein Befehl (`dotnet ef database update`), und die Tabellen stehen in SSMS.

### Mittel
- **Auswahl und Zielfelder.** Man muss sich merken, was angeklickt wurde, und bei jedem weiteren Klick entscheiden: Ziel, andere eigene Figur oder Abbruch?
- **Brett drehen.** Es werden nur die Reihen- und Spaltennummern gespiegelt, aber die Koordinaten-Beschriftung muss dabei mitwandern.
- **Seiten für „Meine Partien“ und „Partie ansehen“.** Eine gespeicherte Partie wird angezeigt, indem man die Züge auf einem neuen Spiel **noch einmal nachspielt**. So muss man keine Brettstellungen speichern, nur die Züge.

### Kompliziert
- **Selbstschach verhindern.** Die Idee („auf einer Kopie ausprobieren“) ist einfach, aber sie hängt von mehreren Teilen ab: Kopie des Bretts, Angriffserkennung und eine saubere Trennung zwischen „möglicher Zug“ und „legaler Zug“.
- **Sonderzüge.** Besonders knifflig waren:
  - **Rochade:** Der König darf nicht im Schach stehen, nicht über ein angegriffenes Feld ziehen und nicht auf eines. Die Rechte verfallen, sobald König oder Turm gezogen wurden, **auch wenn der Turm geschlagen wird**.
  - **En passant:** Das Spiel muss sich den letzten Doppelschritt merken, und das Recht verfällt nach einem Zug. Außerdem kann der Zug den eigenen König ungedeckt lassen, deshalb muss er durch dieselbe Prüfung laufen wie alle anderen.
  - **Umwandlung:** Die Oberfläche muss den Zug anhalten, den Dialog zeigen und erst nach der Auswahl ausführen.
- **Dreifache Stellungswiederholung.** „Gleiche Stellung“ heißt nicht nur gleiche Figuren, sondern auch gleicher Spieler am Zug, gleiche Rochaderechte und gleiche en-passant-Möglichkeit.
- **Schachnotation.** Wenn zwei gleiche Figuren dasselbe Feld erreichen können, muss man mit der Linie oder Reihe unterscheiden (`Nbd2`). Außerdem muss die Notation **vor** dem Zug berechnet werden, das `+` und das `#` kommen aber erst **danach** dazu.
- **Bootstrap gegen Tailwind.** Die Login- und Registrierungsseiten der Vorlage benutzen Bootstrap-Klassen. Ohne Bootstrap sahen sie kaputt aus, deshalb habe ich in der `app.css` die wichtigsten Klassen für Tailwind nachgebaut.
- **Kleinigkeiten, die Zeit gekostet haben:** der Bauer als Emoji statt als Symbol (gelöst mit einem Unicode-Zusatzzeichen), Zeilenenden-Warnung in Visual Studio, die Migration lag in `Data/Migrations` und nicht dort, wo ich sie zuerst vermutet hatte.

---
## Tests
 
Weil die Schachlogik in `Models` weder Blazor noch die Datenbank kennt, kann ich sie ohne Browser automatisch testen. Dafür habe ich **10 Unit-Tests mit xUnit** geschrieben. Sie liegen im Ordner `Chess/Tests`.
 
### Wofür sind die Tests da?
Schachregeln hängen stark zusammen. Ändere ich zum Beispiel etwas an der Schach-Prüfung, kann dadurch plötzlich die Rochade oder das Matt kaputtgehen, ohne dass ich es sofort merke. Die Tests prüfen das in wenigen Sekunden. Wenn alles grün ist, funktionieren die getesteten Regeln noch.
 
### Wie ist ein Test aufgebaut?
Jeder Test folgt dem Muster **Arrange, Act, Assert**:
 
1. **Arrange (vorbereiten):** ein neues Spiel und gegebenenfalls ein paar Züge, um die gewünschte Stellung herzustellen
2. **Act (ausführen):** der Zug oder die Abfrage, die getestet wird
3. **Assert (prüfen):** Vergleich des Ergebnisses mit dem erwarteten Wert. Stimmt es nicht, wird der Test rot.
`[Fact]` über einer Methode markiert sie für xUnit als Test. Jeder Test startet mit einem frischen `new Game()`, deshalb beeinflussen sich die Tests nicht gegenseitig.
 
Ein Beispiel, das Narrenmatt (das kürzeste mögliche Matt):
 
```csharp
[Fact]
public void FoolsMate_IsCheckmate_AndBlackWins()
{
    var game = new Game();                                         // Arrange
 
    Play(game, "f2-f3", "e7-e5", "g2-g4", "d8-h4");                // Act
 
    Assert.Equal(GameStatus.Checkmate, game.Status);               // Assert
    Assert.Equal(PieceColor.Black, game.Winner);
    Assert.Equal("0-1", game.Result);
    Assert.Equal("Qh4#", game.Moves.Last().Notation);
}
```
 
Intern ist das Brett eine Tabelle aus Zahlen (Reihe 0 ist oben die 8. Reihe). Damit die Tests lesbar bleiben, übersetzt die Hilfsmethode `Sq("e2")` den Schachnamen in diese Zahlen, und `Play(...)` spielt mehrere Züge hintereinander und schlägt fehl, wenn einer davon verboten ist.
 
### Was wird getestet?
 
| Datei | Test | Was er prüft |
|---|---|---|
| `MoveRuleTests` | Springer in der Grundstellung | Der Springer auf g1 hat genau zwei Züge (f3, h3), weil e2 durch den eigenen Bauern blockiert ist |
| | Bauer auf der Grundlinie | Der Bauer auf e2 darf ein oder zwei Felder vor |
| | Gegnerische Figur ziehen | Weiß kann keinen schwarzen Bauern ziehen, das Zugrecht bleibt bei Weiß |
| | Gefesselter Bauer | Ein Bauer, der den König vor einem Läufer schützt, darf nicht ziehen (Selbstschach-Prüfung) |
| `SpecialMoveTests` | Kurze Rochade | König landet auf g1, Turm auf f1, Notation `O-O` |
| | Rochade nach Königszug | Ist der König einmal gezogen, auch wenn er zurückkehrt, ist die Rochade verboten |
| | En passant | Der Bauer schlägt auf ein leeres Feld, der übersprungene Bauer verschwindet, Notation `exd6` |
| | Umwandlung | Der Bauer wird zur gewählten Figur, Notation `axb8=N` |
| `GameEndTests` | Narrenmatt | Status Schachmatt, Schwarz gewinnt, Ergebnis `0-1`, Notation `Qh4#` |
| | Dreifache Wiederholung | Nach dem zweiten Mal läuft die Partie weiter, nach dem dritten Mal ist sie Remis |
 
### Tests ausführen
- **Visual Studio:** `Test` → `Test-Explorer` → „Alle Tests ausführen“. Grüne Häkchen bedeuten bestanden.
- **Konsole:** im Repository-Ordner `dotnet test Chess`. Dabei darf die App nicht gleichzeitig laufen, sonst ist die Programmdatei gesperrt.
### Wie ich geprüft habe, dass die Tests wirklich etwas taugen
Alle Tests waren grün, aber ein grüner Test beweist nur etwas, wenn er auch rot werden kann. Deshalb habe ich absichtlich Regeln im Code kaputtgemacht und geschaut, ob die Tests anschlagen. Zum Beispiel:
- Das Rochaderecht nicht mehr verfallen lassen: Der Test „Rochade nach Königszug“ wird rot.
- Die Wiederholungsgrenze von 3 auf 2 senken: Der Test zur dreifachen Wiederholung wird rot.
- Die Selbstschach-Prüfung abschalten: Der Test zum gefesselten Bauern und das Narrenmatt werden rot.
Danach habe ich die Änderungen mit `git restore` zurückgenommen und die Tests wieder grün laufen lassen.
 
### Was die Tests nicht abdecken
Zehn Tests sind Beispiele für die wichtigsten Regeln, keine vollständige Abdeckung. Nicht getestet sind zum Beispiel:
- Patt, die 50-Züge-Regel und zu wenig Material
- Die Notation bei Mehrdeutigkeit (`Nbd2`)
- Das Aufgeben und Remis anbieten
- Die Datenbank und die Oberfläche (Speichern, „Meine Partien“, „Partie ansehen“)
### Warum liegen die Tests im selben Projekt?
Das Projekt ist klein, deshalb habe ich die Tests in den Ordner `Tests` gelegt. Üblich bei größeren Projekten ist ein eigenes Testprojekt (`Chess.Tests`), damit die Test-Pakete nicht in der fertigen App landen. Das ließe sich später leicht nachholen, weil die Tests nur die Klassen aus `Models` benutzen.
 
---
 

## Wie ich mit der KI gearbeitet habe

Ich habe **Claude** als Hilfe benutzt, vor allem für:
- die **Planung**: in welcher Reihenfolge baut man so ein Projekt sinnvoll (Brett, Züge, Schach, Sonderzüge, Remis, Datenbank)
- den **Code** der einzelnen Schritte
- **Erklärungen**, wenn ich etwas nicht verstanden habe (zum Beispiel: Brauche ich eine Datenbank? Was bedeutet diese Warnung?)
- die **Fehlersuche**, wenn ich Fehlermeldungen oder Screenshots geschickt habe

**Meine Regel dabei: Ich kopiere nichts blind.**
- Ich lese den Code, den die KI mir gibt, bevor ich ihn übernehme.
- Wenn ich eine Stelle nicht verstehe, frage ich nach, bis ich sie erklären kann.
- Erst wenn ich verstehe, was da passiert, kopiere ich es ins Projekt.
- Danach teste ich selbst, ob es wirklich so funktioniert, wie es erklärt wurde. Zum Beispiel habe ich das Narrenmatt (`f3 e5 g4 Qh4#`), en passant und die Rochade von Hand durchgespielt.
- Jeder Schritt bekommt einen eigenen Git-Commit. Wenn etwas schiefgeht, kann ich zum letzten funktionierenden Stand zurück.

**Was die KI nicht ersetzt hat:** Sie kann den Code nicht auf meinem Rechner ausführen. Ob etwas funktioniert, finde ich beim Testen heraus, und die Fehlermeldungen schicke ich zurück. Die KI kann sich auch irren, deshalb ist das eigene Prüfen wichtig.

---

## Projekt starten

**Voraussetzungen**
- .NET SDK (passend zur Projektversion)
- Node.js (für Tailwind)
- SQL Server LocalDB (kommt mit Visual Studio)
- Optional: SQL Server Management Studio zum Ansehen der Datenbank

**Schritte**

```powershell
# 1. Projekt klonen und in den Ordner wechseln
git clone <mein-repository>
cd Chess

# 2. Tailwind-Pakete installieren und CSS bauen
cd Chess
npm install
npm run css:build
cd ..

# 3. Datenbank anlegen
dotnet tool update --global dotnet-ef
dotnet ef database update --project Chess
```

Danach das Projekt in Visual Studio mit **Start (https)** ausführen.

Während der Entwicklung kann `npm run css:watch` im Ordner `Chess` laufen, damit das CSS bei Änderungen automatisch neu gebaut wird.

> In der Entwicklung wird keine echte E-Mail verschickt. Nach dem Registrieren zeigt die Seite einen Link zum Bestätigen des Kontos an.

---

## Aktueller Stand

**Von mir getestet und bestätigt**
- [x] Brett, Figuren, Zugregeln
- [x] Schach, Schachmatt
- [x] Rochade, en passant, Umwandlung
- [x] Datenbank angelegt, Identity-Tabellen sichtbar in SSMS
- [x] Migration für `Games` und `Moves` angewendet

**Noch von mir zu testen / zu bestätigen**
- [ ] Remisregeln (Wiederholung, 50-Züge-Regel, zu wenig Material)
- [ ] Notation, geschlagene Figuren, Aufgeben, Remis anbieten
- [ ] Registrieren, Anmelden, Abmelden
- [ ] Partie wird nach jedem Zug gespeichert (Zeilen in `Games` und `Moves`)
- [ ] „Meine Partien“ und „Partie ansehen“

**Bekannte Einschränkungen**
- Beide Seiten spielen am selben Bildschirm.
- Angefangene Partien lassen sich noch nicht fortsetzen, sie erscheinen als „Unvollendet“ und sind nur zum Ansehen da.
- Die 50-Züge-Regel greift automatisch, im echten Schach muss man sie beantragen.
- Keine Schachuhr.

---

## Wie es weitergehen soll

- **Online-Partien** zwischen zwei angemeldeten Benutzern (SignalR), mit Einladung, Lobby und getrennten Buttons für Aufgeben und Remis
- **Computergegner**, zuerst mit zufälligen Zügen, dann mit einer einfachen Bewertung (Minimax)
- Angefangene Partien fortsetzen
- Schachuhr
- Wertungen und Statistiken
- Tests für die Schachlogik (zum Beispiel bekannte Stellungen und Zugzahlen)

---

## Was ich daraus gelernt habe

- Große Aufgaben lassen sich gut in kleine, testbare Schritte teilen.
- Die Logik von der Oberfläche und der Datenbank zu trennen, hat mir später viel Arbeit gespart.
- Einen Fehler findet man leichter, wenn man nach jedem Schritt committet und testet.
- Eine KI ist eine große Hilfe, aber nur, wenn man den Code liest, versteht und selbst prüft.
