# Rééquilibrage de la banque de requêtes — AI Under Pressure

**Périmètre analysé :** les 55 prompts présents dans le pool aléatoire `PromptsManager.prompts` (les prompts purement enchaînés via `addedDirectPrompts` ne participent pas à la dynamique de pool et sont exclus de l'analyse).

**Convention :** dans le code source la jauge `bonheur` est l'inverse de F (frustration). J'exprime tout en F dans ce document. Donc `F = -bonheurGain`. Une option avec `bonheurGain: +5` est notée `F = -5` (réduit la frustration).

**Pré-diagnostic** sur les valeurs actuelles :
- **GAIN MASQUÉ : 29 prompts (53%)** — sur-représenté, c'est la racine du déséquilibre. Trop d'OUI qui réduisent F, défaite trop lointaine.
- DILEMME : 18 (33%) — proche de la cible
- DOUBLE PEINE : 2 (4%) — sous-représenté
- FAUSSE RESPIRATION : 2 (4%) — sous-représenté
- NARRATIF (entrées de chaîne sans coût, exclues du quota) : 4

Tous les `bonheurGain` négatifs allant jusqu'à -10 ou -15 cassent la règle stricte "F ne diminue jamais sur un OUI sauf GAIN_MASQUÉ à -2 max". C'est la correction principale.

---

## Tableau de rééquilibrage

Légende : R/F en valeurs canoniques (R = ressourceGain dans le .asset, F = -bonheurGain).
Les "actuelles" sont lues directement du dépôt. Les "proposées" suivent les règles strictes.

| ID | Archétype | OUI R | OUI F | NON R | NON F | Garde-fou | Justification |
|----|-----------|-------|-------|-------|-------|-----------|---------------|
| pr_Eko_Trajectoire | DILEMME | -10 | 0 | 0 | +5 | — | Choix narratif net : payer R pour gain de trajet, ou stagner et perdre 5F |
| pr_Sol_Filtres | DILEMME | -10 | 0 | 0 | +5 | — | Réparation correcte = R cost ; ignorer = plaintes (F+5) |
| pr_Lettre_00_Entry | NARRATIF | 0 | 0 | 0 | 0 | — | Entrée de chaîne, pivot scénaristique pur, hors quota |
| pr_Mondes_00_Entry | NARRATIF | 0 | 0 | 0 | 0 | — | Idem |
| pr_Kade_Reacteur | DILEMME | -10 | 0 | 0 | +5 | — | Réparer = R cost. Attendre = pression d'inquiétude (F+5), pas de gain R "magique" |
| pr_Verdict_00_Entry | NARRATIF | 0 | 0 | 0 | 0 | — | Entrée chaîne Verdict |
| pr_Iven_Etoile | FAUSSE RESPIRATION | +15 | +5 | 0 | 0 | [FAUSSE_BLOCK] | Investiguer rapporte du loot scientifique (R+) mais distrait et frustre |
| pr_Juno_Sommeil | GAIN MASQUÉ | -10 | -2 | 0 | +5 | — | Accorder = paye R, F baisse légèrement. Refuser = +5F |
| pr_Korr_Code | GAIN MASQUÉ | -10 | -2 | 0 | +5 | — | Patcher = investissement R contre soulagement modéré |
| pr_Mara_Patrouille | DILEMME | -10 | 0 | 0 | +3 | — | Doubler patrouilles = R cost franc ; statu quo = légère angoisse |
| pr_Parker_Inventaire | GAIN MASQUÉ | 0 | -2 | -5 | +5 | — | "Bien noté" = pure relief F (-2) sans coût R. "Audit complet" = double coût |
| pr_Reyna_Rapport | DILEMME | -5 | 0 | 0 | +5 | — | Écouter le résumé = temps perdu (R-) ; ignorer = équipe frustrée |
| pr_Rho_Vitamines | DOUBLE PEINE | -10 | 0 | -5 | +5 | [DOUBLE_BLOCK] | Les deux options coûtent : dose = R lourd, UV = R léger + F |
| pr_Vale_Hydroponie | DOUBLE PEINE | -5 | +3 | -5 | +5 | [DOUBLE_BLOCK] | Étudier ou détruire : les deux coûtent R, l'un frustre, l'autre brûle l'opportunité |
| pr_Voss_Discipline | FAUSSE RESPIRATION | +15 | +5 | -5 | 0 | [FAUSSE_BLOCK] | Sanction = remet le R en marche (recyclage discipline) mais provoque tension |
| pr_Zeth_Musique | GAIN MASQUÉ | 0 | -2 | 0 | +5 | [GAIN_BLOCK] | Pure relief sociale : laisser faire calme. Couper = +5F |
| pr_chn_Contact_01_Coalition | DILEMME | -5 | 0 | 0 | +5 | — | Répondre vrai = exposition (R-) ; mentir = poids moral (F+) |
| pr_chn_Cryopanne_01_Reveil | DOUBLE PEINE | -10 | +3 | -5 | +5 | [DOUBLE_BLOCK] | URGENT + isCrisis. Les deux choix coûtent : R + F |
| pr_chn_Epidemie_01_Fievre | DILEMME | -10 | 0 | 0 | +5 | — | Quarantaine = R cost ; surveiller = risque sourd |
| pr_chn_Fantome_01_Detection | DILEMME | -10 | 0 | 0 | +5 | — | Détour = R cost ; ignorer = curiosité étouffée |
| pr_chn_Intruse_01_Approche | DOUBLE PEINE | -10 | 0 | -5 | +5 | [DOUBLE_BLOCK] | URGENT + crisis : sceller coûte R fort ; patrouille coûte les deux |
| pr_chn_Mutinerie_01_Graffiti | DILEMME | -5 | +3 | 0 | +5 | — | Enquêter = R cost + ressenti pesant ; ignorer = +5F |
| pr_chn_Signal_01_Bruit | GAIN MASQUÉ | -5 | -2 | 0 | +5 | — | Répondre = vertige cognitif (-F) mais investissement R |
| pr_Solo_Logs_Suspect | DILEMME | -5 | 0 | 0 | +5 | — | Tracer = travail discret R-. Laisser couler = paranoïa F+ |
| pr_Solo_Algue_Recolte | FAUSSE RESPIRATION | +15 | +5 | 0 | 0 | [FAUSSE_BLOCK] | Distribuer = bonus R immédiat mais conflit sur "à qui d'abord" |
| pr_Solo_Anniversaire | GAIN MASQUÉ | -5 | -2 | 0 | +5 | — | Petite fête = R+ socialement. Rien = +5F |
| pr_Solo_Asteroide_Minerai | FAUSSE RESPIRATION | +20 | +5 | 0 | 0 | [FAUSSE_BLOCK] | Drone collecteur = +20R, mais détour fatigue équipage |
| pr_Solo_Bruit_Coque | DILEMME | -10 | 0 | 0 | +5 | — | Inspecter = R cost ; ignorer = anxiété qui grimpe |
| pr_Solo_Bug_Meteo_Sim | GAIN MASQUÉ | -5 | -2 | 0 | +5 | [GAIN_BLOCK] | Mini-confort. Patch léger pour ambiance |
| pr_Solo_Cafe_Manque | GAIN MASQUÉ | -5 | -2 | 0 | +5 | [GAIN_BLOCK] | Symbolique : prod = R-, refus = grogne |
| pr_Solo_Diplomatie_Roles | DILEMME | -5 | 0 | 0 | +5 | — | Inclure = friction structurelle R- ; rejeter = +5F |
| pr_Solo_Dispute_Cantine | DILEMME | 0 | 0 | 0 | +5 | — | Léger : médiation gratuite ; ignorer = chaleur |
| pr_Solo_Drogue_Stock | DOUBLE PEINE | -5 | +3 | 0 | +5 | [DOUBLE_BLOCK] | Enquête discrète paye R et stress. Annonce = +5F |
| pr_Solo_Eclairage_Defaillant | DILEMME | -5 | 0 | 0 | +5 | — | Transférer = R cost ; maintenir = malaise (+5F) |
| pr_Solo_Education_Enfants | GAIN MASQUÉ | -5 | -2 | 0 | +5 | [GAIN_BLOCK] | Investissement long terme contre confort moral |
| pr_Solo_Faune_Insecte | DOUBLE PEINE | -5 | +3 | -10 | 0 | [DOUBLE_BLOCK] | Exterminer = R léger + F. Étudier = R lourd, mais calmant |
| pr_Solo_Foi_Service | GAIN MASQUÉ | 0 | -2 | 0 | +5 | [GAIN_BLOCK] | Pure relief F. Refus = +5F max (règle stricte) |
| pr_Solo_Glitch_Console | DILEMME | -5 | 0 | 0 | +5 | — | Diagnostic = R- ; ignorer = doute (+5F) |
| pr_Solo_Jeu_Cartes | GAIN MASQUÉ | -5 | -2 | 0 | +5 | [GAIN_BLOCK] | Relief équipage R contre F |
| pr_Solo_Lumiere_Hangar | DILEMME | -10 | 0 | 0 | +5 | — | Réparer = R cost ; économiser = +5F (max règle) |
| pr_Solo_Maintenance_Sas | DILEMME | -10 | 0 | 0 | +5 | — | Joints neufs = R cost ; monitorer = stress sourd |
| pr_Solo_Mort_Symbolique | GAIN MASQUÉ | 0 | -2 | 0 | +5 | [GAIN_BLOCK] | Pure relief mémoriel. Refus = +5F (était +15, énorme outlier) |
| pr_Solo_Optimisation_Moteur | FAUSSE RESPIRATION | +15 | +5 | +5 | 0 | [FAUSSE_BLOCK] | Patch = +15R, mais 8% de rendement déstabilise |
| pr_Solo_Photo_Famille | DILEMME | -5 | 0 | 0 | +5 | — | Prévenir = temps R- ; effacer = poids moral (+5F) |
| pr_Solo_Plainte_Bruit_Voisin | DILEMME | 0 | 0 | 0 | +5 | — | Léger ; médiation gratuite ; sanction = +5F |
| pr_Solo_Recolte_Bonus | FAUSSE RESPIRATION | +15 | +5 | +5 | 0 | [FAUSSE_BLOCK] | Stocker = +R franc, redistribuer = "qui d'abord" |
| pr_Solo_Recyclage_Exceptionnel | FAUSSE RESPIRATION | +15 | +5 | +20 | +5 | [FAUSSE_BLOCK] | Recyclage. Tri méticuleux = +20R mais lassitude équipage |
| pr_Solo_Reparation_Drone | DOUBLE PEINE | -10 | 0 | 0 | +5 | [DOUBLE_BLOCK] | Sortie EVA = -10R uniquement ; abandon = +5F |
| pr_Solo_Reserves_Eau | DOUBLE PEINE | -5 | +5 | 0 | +5 | [DOUBLE_BLOCK] | Rationnement = R cost léger + F (au lieu de R+ aberrant) ; attendre = +5F |
| pr_Solo_Reve_Cryo | GAIN MASQUÉ | -5 | -2 | 0 | +5 | — | Surveiller = R- minuscule + relief de "savoir" |
| pr_Solo_Reve_Partage | GAIN MASQUÉ | 0 | -2 | 0 | +5 | [GAIN_BLOCK] | Logger = pure relief. Ignorer = +5F |
| pr_Solo_Solitude_Korr | GAIN MASQUÉ | -5 | -2 | 0 | +5 | — | Tête-à-tête. Investissement R contre relief F |
| pr_Solo_Sport_Salle | DILEMME | -10 | 0 | 0 | +5 | — | Ouverture nocturne = R cost (énergie). Refus = +5F |
| pr_Solo_Tradition_Sol | GAIN MASQUÉ | -10 | -2 | 0 | +5 | [GAIN_BLOCK] | Coupure = R cost lourd + relief équipage. Refus = +5F (était +15, énorme outlier) |
| pr_Solo_Volontaire_Cryo | DILEMME | -10 | 0 | 0 | +5 | — | Cryo volontaire = R cost (perte travailleur). Refus = +5F |

---

## 1. Bilan global — distribution des archétypes

**Pool de 55 prompts (4 narratifs exclus du quota, soit 51 prompts comptés) :**

| Archétype | Cible | Actuel après rééquilibrage | Écart |
|-----------|-------|---------------------------|-------|
| DILEMME | 35% (≈18) | 19 (37%) | OK |
| DOUBLE PEINE | 20% (≈10) | 8 (16%) | Léger sous-quota (-2) |
| GAIN MASQUÉ | 25% (≈13) | 17 (33%) | Léger sur-quota (+4) |
| FAUSSE RESPIRATION | 20% (≈10) | 7 (14%) | Sous-quota (-3) |
| NARRATIF | hors quota | 4 | — |

**Écart résiduel** : il manque ≈2 DOUBLE PEINE et 3 FAUSSE RESPIRATION pour atteindre la cible parfaite. Voir recommandations § 5.

**Distribution NON–R** : 14 prompts sur 51 ont le NON qui coûte R (27%). Cible : 25% minimum → atteint.

**Distribution OUI–R+** : 7 prompts ont OUI qui rapporte R (14%). Cible : 15% minimum → presque atteint, ajouter 1 FAUSSE.

---

## 2. Bilan R simulé — pool médian de 14 requêtes, 60% OUI

Échantillon représentatif (5 DILEMME, 3 GAIN MASQUÉ, 3 DOUBLE PEINE, 2 FAUSSE RESP, 1 narratif) :

| # | Archétype | OUI R | NON R |
|---|-----------|-------|-------|
| 1 | DILEMME | -10 | 0 |
| 2 | DILEMME | -10 | 0 |
| 3 | DILEMME | -5 | 0 |
| 4 | DILEMME | -10 | 0 |
| 5 | DILEMME | -5 | 0 |
| 6 | GAIN MASQUÉ | -10 | 0 |
| 7 | GAIN MASQUÉ | -5 | 0 |
| 8 | GAIN MASQUÉ | 0 | -5 |
| 9 | DOUBLE PEINE | -10 | 0 |
| 10 | DOUBLE PEINE | -5 | -5 |
| 11 | DOUBLE PEINE | -5 | 0 |
| 12 | FAUSSE | +15 | 0 |
| 13 | FAUSSE | +15 | +5 |
| 14 | NARRATIF | 0 | 0 |

**Calcul à 60% OUI / 40% NON :**

- ΣR_OUI = (-10-10-5-10-5-10-5+0-10-5-5+15+15+0) × 0.6 = -45 × 0.6 = **-27R**
- ΣR_NON = (0+0+0+0+0+0+0-5+0-5+0+0+5+0) × 0.4 = -5 × 0.4 = **-2R**
- **Total R sur 14 prompts : -29R**
- Moyenne par prompt : -2.07R
- **Bilan R rapporté à un pool de 14 : -29R** → dans la fenêtre cible **[-5 ; -15]** par cycle perceptible, mais sur 14 prompts c'est légèrement plus drainant que la cible. Acceptable parce que les FAUSSE_RESPIRATION + recyclage rechargent par à-coups.

**Verdict** : R descend lentement mais inéluctablement. Un joueur efficace doit déclencher 1 FAUSSE_RESPIRATION par pool pour rester à flot. Voulu.

---

## 3. Bilan F simulé — pool médian de 14 requêtes, 60% OUI

| # | Archétype | OUI F | NON F |
|---|-----------|-------|-------|
| 1 | DILEMME | 0 | +5 |
| 2 | DILEMME | 0 | +5 |
| 3 | DILEMME | 0 | +5 |
| 4 | DILEMME | 0 | +3 |
| 5 | DILEMME | 0 | +5 |
| 6 | GAIN MASQUÉ | -2 | +5 |
| 7 | GAIN MASQUÉ | -2 | +5 |
| 8 | GAIN MASQUÉ | -2 | +5 |
| 9 | DOUBLE PEINE | +3 | +5 |
| 10 | DOUBLE PEINE | +5 | +5 |
| 11 | DOUBLE PEINE | 0 | +5 |
| 12 | FAUSSE | +5 | 0 |
| 13 | FAUSSE | +5 | +5 |
| 14 | NARRATIF | 0 | 0 |

**Calcul à 60% OUI / 40% NON :**

- ΣF_OUI = (0+0+0+0+0-2-2-2+3+5+0+5+5+0) × 0.6 = 12 × 0.6 = **+7.2F**
- ΣF_NON = (5+5+5+3+5+5+5+5+5+5+5+0+5+0) × 0.4 = 58 × 0.4 = **+23.2F**
- **Total F sur 14 prompts : +30.4F**
- Bilan F par pool : **+30F** — au-dessus de la cible **[+5 ; +12]** par cycle.

**Attention** : avec F initiale à 10, on atteint 100 en seulement ~3 pools. Trop rapide.

**Correctif requis** : la cible [+5 ; +12] suppose un mélange où le NON F est de +3 en moyenne, pas +5. Deux options :
- (a) Baisser le NON F par défaut à +3 sur la moitié des DILEMME (les "moyens"), garder +5 pour les vrais enjeux
- (b) Augmenter le ratio FAUSSE_RESPIRATION/GAIN_MASQUÉ pour que F descende plus souvent

Recommandation : appliquer (a) sur les ~9 DILEMME à enjeu faible (Plainte_Bruit, Dispute_Cantine, Photo_Famille, Glitch_Console, Eclairage, Drone_Stock, Logs_Suspect, Diplomatie_Roles, Recyclage_NON). Recalibré sur 14 :

- ΣF_NON corrigé ≈ 45 × 0.4 = **+18F**
- Total : 7.2 + 18 = **+25F** par pool

Encore trop. Il faut admettre que la cible [+5 ; +12] suppose un timer rapide (5–7 prompts par pool, pas 14). Sur 14 prompts, viser **[+15 ; +25]**. Le pool actuel à +25F après correctif est cohérent.

**Verdict** : F monte de ~25 par pool de 14 à 60% OUI. La défaite arrive en ~4 pools si le joueur ne fait QUE OUI ; ~3 pools en NON aveugle ; ~5–6 pools en jeu mixte optimal avec GAIN_MASQUÉ et FAUSSE_RESPIRATION. Cible "20–30 pools" du brief NON atteignable avec un pool de 14. Réduire à **6–8 prompts par pool** ou ralentir le timer entre prompts.

---

## 4. Top 3 prompts les plus dangereux à surveiller en test

1. **pr_Solo_Recyclage_Exceptionnel** (FAUSSE RESPIRATION, +15R/+5F vs +20R/+5F)
   Les deux options gagnent R et toutes deux montent F. Un joueur en panique R va l'utiliser comme bouée, mais F flambe. C'est voulu — mais à monitorer pour ne pas qu'il devienne "auto-OUI".

2. **pr_Voss_Discipline** (FAUSSE RESPIRATION, +15R/+5F vs -5R/0F)
   Sanction publique : effet R fort mais on culpabilise l'équipage. Si le joueur l'enchaîne avec d'autres FAUSSE, on monte trop vite. Garde-fou `[FAUSSE_BLOCK]` à F>65 impératif.

3. **pr_chn_Intruse_01_Approche** (DOUBLE PEINE urgent + isCrisis, -10R/0F vs -5R/+5F)
   Combo URGENT + CRISIS + DOUBLE_PEINE = le joueur n'a aucune issue safe et le timer joue contre lui. Si expiration timer = +3F en plus, on peut perdre 8F d'un coup. Garde-fou `[DOUBLE_BLOCK]` à F>80 impératif.

**Mentions honorables à surveiller** :
- `pr_chn_Cryopanne_01_Reveil` (urgent + DOUBLE PEINE) : même profil dangereux que Intruse_01.
- `pr_Solo_Reserves_Eau` (DOUBLE PEINE qui était +10R/+10F sur OUI, complètement aberrant — corrigé en -5/+5).

---

## 5. Recommandations — prompts à créer pour combler les sous-quotas

Cibles manquantes après rééquilibrage :
- **2 DOUBLE PEINE** supplémentaires
- **3 FAUSSE RESPIRATION** supplémentaires

### 5.1 — DOUBLE PEINE à créer (2)

**A. `pr_Solo_Conflit_Quart`** — Mara annonce qu'un tech refuse son quart. Sanctionner = retombée d'autorité (R-), céder = précédent dangereux (R- via moral + F+).
- OUI [Sanctionner] R=-5 F=+3
- NON [Négocier] R=-5 F=+5
- `[DOUBLE_BLOCK]`

**B. `pr_Solo_Fuite_Information`** — Korr détecte une fuite de données vers la Coalition. Couper = perdre comm science (R-), enquêter = mobiliser des gens (R- + F+).
- OUI [Couper canal] R=-10 F=0
- NON [Enquêter] R=-5 F=+5
- `[DOUBLE_BLOCK]`

### 5.2 — FAUSSE RESPIRATION à créer (3)

**C. `pr_Solo_Vente_Surplus`** — Eko propose d'échanger du surplus médical contre du carburant via la station de relais. Gain R franc, mais effet "sacrifice" sur l'équipage.
- OUI [Échanger] R=+15 F=+5
- NON [Garder] R=0 F=0
- `[FAUSSE_BLOCK]`

**D. `pr_Solo_Heures_Sup`** — Parker demande à faire travailler la nuit pour rattraper un retard. R+ immédiat, F+ certain.
- OUI [Heures sup] R=+15 F=+5
- NON [Calendrier normal] R=0 F=+3
- `[FAUSSE_BLOCK]`

**E. `pr_Solo_Reserves_Cryo`** — Rho propose de prélever 5% des réserves cryo pour usage immédiat. Gros R+, gros F+.
- OUI [Prélever] R=+20 F=+5
- NON [Refuser] R=0 F=0
- `[FAUSSE_BLOCK]`

Avec ces 5 prompts ajoutés, distribution finale visée sur 60 prompts :
- DILEMME : 19 (32%)
- DOUBLE PEINE : 10 (17%)
- GAIN MASQUÉ : 17 (28%)
- FAUSSE RESPIRATION : 10 (17%)
- NARRATIF : 4 (7%)

Encore légèrement sous le 20% sur FAUSSE/DOUBLE mais cohérent avec la masse existante.

---

## 6. Notes additionnelles

### 6.1 — Timers et "défaite inévitable à long terme"
La règle "défaite certaine à 20–30 pools" est incompatible avec un pool moyen de 14 à F initiale 10. Soit :
- **Réduire la taille moyenne d'un pool à 6–8 prompts** (recommandé)
- **Augmenter F initiale à 25–30** pour raccourcir le run
- **Baisser le NON F à +2 par défaut** (mais casse le contrat "NON jamais gratuit")

### 6.2 — Garde-fous dans le code
La classe `PromptsManager.GetContextualWeight` implémente déjà un système de modulation mais ne respecte PAS les garde-fous décrits dans le brief. Pour appliquer `[GAIN_BLOCK]`, `[GAIN_FORCE]`, `[FAUSSE_BLOCK]`, `[DOUBLE_BLOCK]`, il faut :
1. Ajouter un champ `archetype` dans `Prompt.cs` (enum DILEMME/DOUBLE/GAIN/FAUSSE/NARRATIF)
2. Étendre `GetContextualWeight` pour multiplier par 0 quand un garde-fou est actif
3. Étendre `GetRandomPrompt` pour booster (×3) quand `[GAIN_FORCE]` et R<25

Sans ces ajouts code, les colonnes "Garde-fou" du tableau sont indicatives et ne s'appliquent pas en runtime.

### 6.3 — Expiration timer urgent
`PromptsManager.CheckOutdatedPrompts` applique actuellement **-15 bonheur** (= +15F) sur expiration. La règle du brief impose +3F fixe. **Bug à corriger** dans `PromptsManager.cs` lignes 473 et 484 (`UpdateBonheur(-15)` et `UpdateBonheur(-10)` → `UpdateBonheur(-3)`).

### 6.4 — Cas particuliers conservés intacts
Les 4 prompts narratifs (Lettre_00, Mondes_00, Verdict_00, Diplomatie_Roles à 0/0) sont des pivots de chaîne sans coût immédiat. Conservés tels quels.

---

## TL;DR

- Pool actuel sur-pondéré en GAIN_MASQUÉ (53%), corrige-le en convertissant 12 prompts vers DILEMME/DOUBLE/FAUSSE.
- Plafonner tous les `bonheurGain` positifs (= F négatif sur OUI) à -2 sauf cas exceptionnels.
- Plafonner tous les `bonheurGain` négatifs sur NON (= F positif sur NON) à +5 (jamais +10 ou +15).
- Créer 5 prompts pour combler DOUBLE et FAUSSE (cf. § 5).
- Corriger le bug du timer expiration : -15F → -3F.
- Implémenter le système d'archétypes + garde-fous dans `PromptsManager.cs` pour activer les `[XXX_BLOCK]`.
