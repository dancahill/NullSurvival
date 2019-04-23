using System.Collections.Generic;

public class AnimalStats
{
	public class Stats
	{
		public string name;
		public float baseHealth;
		public float baseDamage;
		public float bodyLength;
		public float bodyMass;
		public float maxSpeed;
		public float turnSpeed;
		public float chanceToSpawn;

		public Stats()
		{
			name = "Unnamed";
			chanceToSpawn = 100;
		}
	}
	private static readonly float KPHtoMS = 1000f / 3600f;
	private static readonly float MPHtoMS = KPHtoMS * 1.60934f;
	public static List<Stats> Animals = new List<Stats>
	{
		// modern animals
		new Stats { name="Brown Bear",                 baseHealth=100, baseDamage=25, maxSpeed = 20*MPHtoMS, turnSpeed = 2.0f, chanceToSpawn=50  },
		new Stats { name="Bull",                                                                                                        },
		new Stats { name="Butterfly",                  baseHealth= 10, baseDamage= 0, maxSpeed = 0.5f,       turnSpeed = 2.0f, chanceToSpawn=50  },
		new Stats { name="Chicken",                    baseHealth= 20,                                                                           },
		new Stats { name="Chicken v2",                 baseHealth= 20,                                                                           },
		new Stats { name="Chicken v3",                 baseHealth= 20,                                                                           },
		new Stats { name="Common Frog",                baseHealth= 10,                                                                           },
		new Stats { name="Common Frog v2",             baseHealth= 10,                                                                           },
		new Stats { name="Common Frog v3",             baseHealth= 10,                                                                           },
		new Stats { name="Cow",                                                                                                                  },
		new Stats { name="Crab",                       baseHealth= 10,                                                                           },
		new Stats { name="Crocodile",                                                                                                   },
		new Stats { name="Deer",                                                                                                        },
		new Stats { name="Deer (Male)",                                                                                                 },
		new Stats { name="Fire Salamander",           baseHealth= 20,                                                                            },
		new Stats { name="Goat",                                                                                                        },
		new Stats { name="Great White Shark",                                                                                           },
		new Stats { name="Iron Age Pig",                                                                                                },
		new Stats { name="Iron Age Pig v2",                                                                                             },
		new Stats { name="Mountain Goat (Ibex)",                                                                                        },
		new Stats { name="Octopus",                   baseHealth= 20,                                                                   },
		new Stats { name="Octopus v2",                baseHealth= 20,                                                                   },
		new Stats { name="Perch",                     baseHealth= 20,                                                                   },
		new Stats { name="Pike",                      baseHealth= 20,                                                                   },
		new Stats { name="Rat",                       baseHealth= 10,                                                                   },
		new Stats { name="Salmon",                    baseHealth= 20,                                                                   },
		new Stats { name="Salmon (old)",              baseHealth= 20,                                                                   },
		new Stats { name="Scorpion",                  baseHealth= 20,                                                                   },
		new Stats { name="Scorpion (Yellow Fattail)", baseHealth= 20,                                                                   },
		new Stats { name="Snail",                     baseHealth= 10,                                                                   },
		new Stats { name="Swan Goose",                baseHealth= 20,                                                                   },
		new Stats { name="Viper",                     baseHealth= 50,                                                                   },
		new Stats { name="Wild Boar",                 baseHealth=100,                                                                   },
		new Stats { name="Wild Rabbit",               baseHealth= 20,                                                                   },
		new Stats { name="Wild Rabbit v2",            baseHealth= 20,                                                                   },
		new Stats { name="Wolf",                      baseHealth= 75,                                                                   },
		// prehistoric animals
		new Stats { name="Ankylosaurus",      baseHealth=100, baseDamage=20,                                          chanceToSpawn=50  },
		new Stats { name="Argentinosaurus",   baseHealth=200, baseDamage=50, maxSpeed = 20*KPHtoMS, turnSpeed = 0.4f, chanceToSpawn=20  },
		new Stats { name="Brontosaurus",      baseHealth=150, baseDamage=40, maxSpeed = 20*KPHtoMS, turnSpeed = 0.4f, chanceToSpawn=40  },
		new Stats { name="Carnotaurus",       baseHealth=100, baseDamage=20,                                          chanceToSpawn=30  },
		new Stats { name="Megalodon",         baseHealth=100, baseDamage=25,                                          chanceToSpawn=10  },
		new Stats { name="Parasaurolophus",   baseHealth=100, baseDamage= 0,                                          chanceToSpawn=100 },
		new Stats { name="Spinosaurus",       baseHealth=125, baseDamage=30,                                          chanceToSpawn=20  },
		new Stats { name="Stegosaurus",       baseHealth=100, baseDamage=20, maxSpeed = 7*KPHtoMS,  turnSpeed = 0.4f, chanceToSpawn=60  },
		new Stats { name="Triceratops",       baseHealth=100, baseDamage=20,                                          chanceToSpawn=60  },
		new Stats { name="Tyrannosaurus Rex", baseHealth=150, baseDamage=35, maxSpeed = 29*KPHtoMS, turnSpeed = 1.0f, chanceToSpawn=20  }, // 29 kph
		new Stats { name="Velociraptor",      baseHealth= 50, baseDamage=10, maxSpeed = 30*KPHtoMS, turnSpeed = 3.0f, chanceToSpawn=40  }  // 64 kph according to google, but way too fast for the game
	};
}
