using GuiTest.Commands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GuiTest.ViewModels {
  public static class Configuration {
    public const int GeneMutationProbability = 30;
    public const int GeneCount = 1000;
    public const int PopulationSize = 1000;
    public const int CrossoversPerPopulation = 1000;
    public const int MutationsPerGeneration = 1000;
  }

  public class Gene {
    public int Allele { get; }

    public Gene(int allele) {
      Allele = allele;
    }

    public Gene Mutate() {
      return new Gene((Allele + 1) % 2);
    }
  }

  public class Genotype {
    public Gene[] Genes { get; }

    public int Fitness { get; }

    public Genotype(Gene[] genes) {
      Genes = genes;
      Fitness = genes.Sum(g => g.Allele);
    }

    public Genotype Crossover(Genotype partner) {
      var position = new Random(Guid.NewGuid().GetHashCode()).Next(Genes.Length);
      var genes = new Gene[Genes.Length];

      // TODO: Array.Copy?
      for(var i = 0; i < genes.Length; ++i) {
        genes[i] = (i < position) ? Genes[i] : partner.Genes[i];
      }

      return new Genotype(genes);
    }

    public Genotype Mutate() {
      var random = new Random();
      var genes = new Gene[Genes.Length];

      for(var i = 0; i < genes.Length; ++i) {
        genes[i] = (random.Next(1, 101) <= Configuration.GeneMutationProbability) ? Genes[i].Mutate() : Genes[i];
      }

      return new Genotype(genes);
    }

    public static Genotype CreateRandom(int count) {
      var random = new Random(Guid.NewGuid().GetHashCode());
      return new Genotype(Enumerable.Repeat(1, count).Select(i => new Gene(random.Next(2))).ToArray());
    }

    public override string ToString() {
      return string.Join(" ", Genes.Select(g => g.Allele));
    }
  }

  public class Population {
    private readonly Genotype[] _individuals;

    public Genotype Best => _individuals.Aggregate(_individuals[0], (memo, individual) => (memo.Fitness > individual.Fitness) ? memo : individual);

    public int Average => (int)_individuals.Average(g => g.Fitness);
    public int Min => _individuals.Min(g => g.Fitness);
    public int Max => _individuals.Max(g => g.Fitness);

    private Population(Genotype[] individuals) {
      _individuals = individuals;
    }

    public static Population CreateRandom() {
      return new Population(Enumerable.Repeat(Configuration.GeneCount, Configuration.PopulationSize).Select(Genotype.CreateRandom).ToArray());
    }

    public Population Evolve() {
      var descendants = new Genotype[Configuration.CrossoversPerPopulation + Configuration.MutationsPerGeneration];

      for(var i = 0; i < descendants.Length; ++i) {
        var random = new Random(Guid.NewGuid().GetHashCode());
        if(i < Configuration.CrossoversPerPopulation) {
          descendants[i] = _individuals[random.Next(_individuals.Length)].Crossover(_individuals[random.Next(_individuals.Length)]);
        } else {
          descendants[i] = _individuals[random.Next(_individuals.Length)].Mutate();
        }
      }


      //Parallel.For(0, descendants.Length, i => {
      //  var random = new Random(Guid.NewGuid().GetHashCode());
      //  if(i < Configuration.CrossoversPerPopulation) {
      //    descendants[i] = _individuals[random.Next(_individuals.Length)].Crossover(_individuals[random.Next(_individuals.Length)]);
      //  } else {
      //    descendants[i] = _individuals[random.Next(_individuals.Length)].Mutate();
      //  }
      //});

      return new Population(descendants.Concat(_individuals).OrderBy(g => -g.Fitness).Take(Configuration.PopulationSize).ToArray());
    }
  }

  public struct Statistics {
    public int Max { get; set; }
    public int Min { get; set; }
    public int Average { get; set; }
    public int Generation { get; set; }
    public long EvolutionTimeMs { get; set; }

    public override string ToString() {
      return $"Max: {Max}, Min: {Min}, Average: {Average}, Generation: {Generation}, Time: {EvolutionTimeMs} ms";
    }
  }

  public class GeneticsViewModel : BaseViewModel {
    private Genotype _best;
    private bool _ready = true;

    private Statistics _statistics;

    public Genotype Best {
      get => _best;
      set {
        _best = value;
        OnPropertyChanged();
      }
    }

    public bool Ready {
      get => _ready;
      set {
        _ready = value;
        OnPropertyChanged();
      }
    }

    public Statistics Statistics {
      get => _statistics;
      set {
        _statistics = value;
        OnPropertyChanged();
      }
    }

    public ICommand Evolve { get; }

    public GeneticsViewModel() {
      Evolve = new AsyncRelayCommand(_Evolve);
    }

    private async Task _Evolve(object args) {
      var stopwatch = Stopwatch.StartNew();
      Ready = false;
      var population = await Task.Run(() => Population.CreateRandom());

      for(var i = 1; i <= 100; ++i) {
        population = await Task.Run(() => population.Evolve());
        Best = await Task.Run(() => population.Best);
        Statistics = await Task.Run(() => new Statistics {
          Average = population.Average,
          Max = population.Max,
          Min = population.Min,
          Generation = i,
          EvolutionTimeMs = stopwatch.ElapsedMilliseconds
        });
        //await Task.Delay(100);
      }
      Ready = true;
    }
  }
}
