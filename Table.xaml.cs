using System.Collections.Generic;
using System.Linq;

namespace SimpleDistr
{
    /// <summary>
    /// Interaction logic for Table.xaml
    /// </summary>
    public partial class Table
    {
        private readonly int _proc;
        private readonly List<Unit> _allUnits;
        private List<Unit> _allUnitsOrdered;
        private readonly List<Binding> _bindings;
        //private List<Unit> _sheduledUnits = new List<Unit>();
        private readonly List<Processor> _processors = new List<Processor>();

        public Table(List<Unit> units, List<Binding> bindings, int proc)
        {
            InitializeComponent();
            _proc = proc;
            _bindings = bindings;
            _allUnits = units;
            for (var i = 0; i < proc; i++)
                _processors.Add(new Processor(i));
            Sheduler();
            FillTable();
        }

        public void Sheduler()
        {
            foreach (var unit in _allUnits)
                unit.FinishTime = -1;

            _allUnitsOrdered = _allUnits.OrderBy(u => u.Level).ToList();

            while (_allUnitsOrdered.Count > 0)
            {
                var applicant = _allUnitsOrdered.First();
                _allUnitsOrdered.RemoveAt(0);

                var parents = _allUnits.FindAll(
                    u => u.Binds.Any(
                        bId => _bindings.Find(b => b.Index == bId).Units[1] == applicant.Index
                        )
                    );

                // No parents
                if (parents.Count == 0)
                {
                    // Find best variant (processor)
                    var procToLoad = 0;
                    foreach (var processor in _processors)
                        if (processor.LeftTime < _processors[procToLoad].LeftTime)
                            procToLoad = processor.Index;

                    // Load it
                    _processors[procToLoad].QueueList.Add(applicant);
                    _processors[procToLoad].LeftTime += applicant.Complexity;
                    applicant.FinishTime = _processors[procToLoad].LeftTime;
                    continue;
                }

                // There are parents
                // Check each queue for time to start this task
                var startTimeOnEachProc = new int[_proc];

                // Fill array with time they are currently free
                for (var procId = 0; procId < startTimeOnEachProc.Length; procId++)
                    startTimeOnEachProc[procId] = _processors[procId].LeftTime;

                foreach (var unit in parents)
                {
                    var minimumStartTimeOnOther = unit.FinishTime
                                                  +
                                                  _bindings.Find(
                                                      b =>
                                                          b.Units.Contains(unit.Index) &&
                                                          b.Units.Contains(applicant.Index)).Delay;

                    // Compare all possible variants
                    for (var procId = 0; procId < _proc; procId++)
                    {
                        // We don't need to send data to the same proc we send from
                        if (_processors[procId].QueueList.Contains(unit)) continue;
                        if (startTimeOnEachProc[procId] <= minimumStartTimeOnOther)
                            startTimeOnEachProc[procId] = minimumStartTimeOnOther;
                    }
                }

                // Find best variant (processor index)
                var minimumTimeStartIs = 0;
                for (var proc = 0; proc < startTimeOnEachProc.Length; proc++)
                    if (startTimeOnEachProc[proc] <= startTimeOnEachProc[minimumTimeStartIs])
                        minimumTimeStartIs = proc;

                // Load this processor
                _processors[minimumTimeStartIs].QueueList.Add(applicant);
                _processors[minimumTimeStartIs].LeftTime = startTimeOnEachProc[minimumTimeStartIs] + applicant.Complexity;
                applicant.FinishTime = _processors[minimumTimeStartIs].LeftTime;
            }
            
        }

        private void FillTable()
        {
            var allTicks = new List<ProcTable>();
            var maxFinishTime = _processors.Select(t => t.LeftTime).Concat(new[] {0}).Max();

            for (var i = 0; i < maxFinishTime; i++)
            {
                string p0;
                string p1;
                string p2;
                string p3;

                var task = _processors[0].QueueList.Find(u => u.FinishTime > i);
                if (task == null || i < task.FinishTime - task.Complexity)
                    p0 = "";
                else
                    p0 = task.Index.ToString();

                task = _processors[1].QueueList.Find(u => u.FinishTime > i);
                if (task == null || i < task.FinishTime - task.Complexity)
                    p1 = "";
                else
                    p1 = task.Index.ToString();

                task = _processors[2].QueueList.Find(u => u.FinishTime > i);
                if (task == null || i < task.FinishTime - task.Complexity)
                    p2 = "";
                else
                    p2 = task.Index.ToString();

                task = _processors[3].QueueList.Find(u => u.FinishTime > i);
                if (task == null || i < task.FinishTime - task.Complexity)
                    p3 = "";
                else
                    p3 = task.Index.ToString();

                allTicks.Add(new ProcTable()
                {
                    Tick = i.ToString(),
                    P0 = p0,
                    P1 = p1,
                    P2 = p2,
                    P3 = p3
                });
            }

            ListViewTable.ItemsSource = allTicks;

        }
        
    }
}
