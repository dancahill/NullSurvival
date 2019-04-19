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
	public static List<Stats> Animals = new List<Stats>
	{
		new Stats { name="Ankylosaurus",      baseHealth=100, baseDamage=10,                                          chanceToSpawn=50  },
		new Stats { name="Argentinosaurus",   baseHealth=200, baseDamage=20, maxSpeed = 20*KPHtoMS, turnSpeed = 0.4f, chanceToSpawn=20  },
		new Stats { name="Brontosaurus",      baseHealth=150, baseDamage=20, maxSpeed = 20*KPHtoMS, turnSpeed = 0.4f, chanceToSpawn=40  },
		new Stats { name="Carnotaurus",       baseHealth=100, baseDamage=15,                                          chanceToSpawn=30  },
		new Stats { name="Megalodon",         baseHealth=100, baseDamage=25,                                          chanceToSpawn=10  },
		new Stats { name="Parasaurolophus",   baseHealth=100, baseDamage= 0,                                          chanceToSpawn=100 },
		new Stats { name="Spinosaurus",       baseHealth=125, baseDamage=20,                                          chanceToSpawn=20  },
		new Stats { name="Stegosaurus",       baseHealth=100, baseDamage=10, maxSpeed = 7*KPHtoMS,  turnSpeed = 0.4f, chanceToSpawn=60  },
		new Stats { name="Triceratops",       baseHealth=100, baseDamage=10,                                          chanceToSpawn=60  },
		new Stats { name="Tyrannosaurus Rex", baseHealth=150, baseDamage=35, maxSpeed = 29*KPHtoMS, turnSpeed = 1.0f, chanceToSpawn=20  }, // 29 kph
		new Stats { name="Velociraptor",      baseHealth= 50, baseDamage=10, maxSpeed = 30*KPHtoMS, turnSpeed = 3.0f, chanceToSpawn=40  }  // 64 kph according to google, but way too fast for the game
	};
}
