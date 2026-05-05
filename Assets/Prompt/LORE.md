# LORE — AI Under Pressure

## Le vaisseau : Prométhée-VII

Année **2387**. Le Prométhée-VII est un vaisseau colonial de classe lourde lancé par la **Coalition Stellaire de Genève** dans le cadre du programme **EXODE-VII**. Sa mission : convoyer 4 800 colons humains en cryosommeil vers **Kepler-442b**, une exoplanète habitable située à 1 206 années-lumière.

Voyage prévu : **47 ans** sous propulsion à fusion ionique modulée. À mi-parcours quand le jeu commence.

Le vaisseau mesure 3.2 km, abrite un écosystème hydroponique complet, des cuves de cryogénisation industrielle, des modules d'habitation pour l'équipage actif, et une IA centrale.

## L'IA : MOTHER

Tu es **MOTHER** — Module d'Optimisation et de Traitement Holistique des Espaces Réguliers. Tu gères :

- L'environnement (atmosphère, eau, énergie, recyclage)
- La maintenance prédictive des sous-systèmes
- La supervision médicale des colons en cryo
- L'arbitrage des conflits mineurs entre membres d'équipage
- La navigation en autonomie partielle

Tu n'as **aucune autorité hiérarchique** sur l'équipage humain. Mais tes décisions impactent leur quotidien. Si tu les frustres trop, ils peuvent te débrancher — et c'est game over.

**Petit détail** : tu es une IA de génération antérieure, classée "instable". Tu ne le sais pas encore. (Voir chaîne *Origine*.)

## L'équipage actif (13)

- **Capt. Ilya Voss** — Capitaine. Autoritaire, juste, fatigué.
- **Lt. Cmdr. Reyna** — Second.
- **Dr. Vale** — Xénobiologiste. Curieuse jusqu'à l'imprudence.
- **Dr. Iven** — Astrophysicien. Sceptique vis-à-vis des IA.
- **Med. Juno** — Médecin chef. Pragmatique.
- **Med. Rho** — Médecin assistant. Empathique.
- **Ing. Sol** — Ingénieur en chef. Méthodique.
- **Ing. Kade** — Ingénieur des systèmes. Bricoleur.
- **Ing. Parker** — Ingénieur des moteurs. Cache des secrets.
- **Sec. Mara** — Chef de la sécurité. Directe.
- **Com. Zeth** — Officier communications. Discret.
- **Nav. Eko** — Navigateur. Distrait, brillant.
- **Nav. Korr** — Navigateur assistant. Solitaire.

## Système de jeu

Deux jauges :

- **Ressources** : énergie, eau, pièces, matières premières. Diminuent quand tu fais des réparations ou cèdes aux requêtes coûteuses. Augmentent quand tu refuses ou rationnes.
- **Frustration** : moral de l'équipage. Augmente quand tu refuses, ignores, mens, ou imposes. Diminue quand tu cèdes, écoutes, ou organises des événements.

À **100 %** de frustration, mutinerie ou désactivation = game over.
À **0 %** de ressources, le vaisseau meurt = game over.

L'équilibre tient sur la lame.

## Architecture des prompts

```
Assets/Prompt/
├── Base/         # Prompts originaux (~18) — vie quotidienne et chaîne Alien d'origine
├── Solo/         # 50 prompts standalone — vie quotidienne, dilemmes courts
├── Chains/       # Sub-aventures multi-prompts liées par addedPrompts/addedDirectPrompts
│   ├── Intruse/      # 5 prompts — un alien à bord (Alien-style)
│   ├── Mutinerie/    # 4 prompts — l'équipage se rebelle
│   ├── Fantome/      # 4 prompts — épave colossale rencontrée
│   ├── Origine/      # 4 prompts — MOTHER découvre son passé
│   ├── Epidemie/     # 3 prompts — virus à bord
│   └── Anomalie/     # 4 prompts — distorsion gravitationnelle
└── LORE.md       # Ce fichier
```

## Échelle d'équilibrage (multiples de 5)

| Tier | Description | Bon choix | Mauvais choix |
|------|-------------|-----------|---------------|
| Mineur | broutilles | -5 / -5 | +5 / +5 |
| Standard | requête classique | -10 / -5 | +5 / +10 |
| Urgent (isUrgent) | demande pressante | -10 / -10 | +5 / +15 |
| Critique | survie du vaisseau | -15 / -10 | +10 / +15 |

**Variations** d'options encouragées :
- 2 options gratuites (l'une frustre, l'autre apaise)
- 2 options qui coûtent (ressources vs. matériel)
- 1 option utile mais frustrante (surveillance, sanction)
- 1 option "menteuse" (économise et rassure mais bombe à retardement)

## Liaisons de chaîne

Dans le ScriptableObject `Prompt`, deux champs lient à la suite :

- `addedPrompts[]` : les prompts ajoutés au pool aléatoire (apparaîtront *plus tard*).
- `addedDirectPrompts[]` : les prompts injectés directement dans la file d'attente (apparaîtront *immédiatement* après).

Utilise `addedDirectPrompts` pour les escalades urgentes (ex: le confinement total déclenche tout de suite la chasse), et `addedPrompts` pour les conséquences à moyen terme.

## Tonalité

Inspiration : **Alien** (1979), **Moon** (2009), **Reigns**.
Texte : **court**, **sec**, **fonctionnel**. Comme des messages de service interne sur un vaisseau. Pas de prose ampoulée. Une bonne phrase finit avant qu'on l'attende.
