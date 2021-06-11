using System;
using System.Linq;
using ASD;
using ASD.Graphs;

namespace Lab10
{
    public struct MinimalPathBetweenTerminalsArgs
    {
        public Graph InputGraph { get; set; }
        public int[] TerminalVertices { get; set; }
        public bool SolutionExists { get; set; }
        public double? ExpectedSumOfEdgesWeights { get; set; }
        public bool VerifyPath { get; set; }
        public double TimeLimit { get; set; }
        public Exception ExpectedException { get; set; }
        public string Description { get; set; }
    }

    public class MinimalPathBetweenTerminalsTestCase : TestCase
    {
        protected readonly Graph _inputGraph, _graphCopy;
        protected readonly int[] _terminalVertices;
        protected readonly bool _solutionExists;
        protected readonly double? _expectedSumOfEdgesWeight;
        protected readonly bool _verifyPath;
        protected (double? sumOfEdgesWeight, Edge[] usedPath) _result;

        public MinimalPathBetweenTerminalsTestCase(MinimalPathBetweenTerminalsArgs args) 
            : base(args.TimeLimit, args.ExpectedException, args.Description)
        {
            _inputGraph = args.InputGraph;
            _graphCopy = args.InputGraph.Clone();
            _terminalVertices = args.TerminalVertices;
            _solutionExists = args.SolutionExists;
            _expectedSumOfEdgesWeight = args.ExpectedSumOfEdgesWeights;
            _verifyPath = args.VerifyPath;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            _result = ((Lab10) prototypeObject).Version1(_inputGraph, _terminalVertices);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (!_inputGraph.IsEqual(_graphCopy))
                return (Result.WrongResult, "Input graph has changed!");

            if (!_solutionExists)
            {
                if (_result.usedPath != null && _result.usedPath.Any())
                    return (Result.WrongResult, "For non existing solution usedPath should be null or empty array");
                if (_result.sumOfEdgesWeight == null)
                    return (Result.Success,
                        "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
            }

            if (!_result.sumOfEdgesWeight.HasValue)
                return (Result.WrongResult, "No solution found, but solution exists. ");

            if (_verifyPath)
            {
                var sumOfUsedEdges = _result.usedPath.Sum(s => s.Weight);
                if(_result.sumOfEdgesWeight != sumOfUsedEdges)
                    return (Result.WrongResult, $"Sum of edges weight used in building path ({sumOfUsedEdges}) is different than returned: sumOfEdgesWeight({_result.sumOfEdgesWeight})");

                if (!AreTerminalsInResult(_inputGraph.VerticesCount, _result.usedPath))
                    return (Result.WrongResult, $"All terminal vertices should be in output vertices.");

                if (_result.sumOfEdgesWeight > _expectedSumOfEdgesWeight && AreTerminalsInResult(_inputGraph.VerticesCount, _result.usedPath))
                    return (Result.WrongResult, $"Exists solution with better sum off edges weights: {_expectedSumOfEdgesWeight} / {_result.sumOfEdgesWeight}");

                if (_result.sumOfEdgesWeight < _expectedSumOfEdgesWeight && AreTerminalsInResult(_inputGraph.VerticesCount, _result.usedPath))
                    return (Result.WrongResult, $"Result is better than expected (actual: {_result.sumOfEdgesWeight}, expected: {_expectedSumOfEdgesWeight}). Verify if all terminals are connected. If it is so please contact with teacher");
            }

            if (_result.sumOfEdgesWeight.Value == _expectedSumOfEdgesWeight)
                return (Result.Success, "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");


            return (Result.WrongResult, $"Result not expected. Contact with teacher.");
        }

        public bool AreTerminalsInResult(int numberOfVertices, Edge[] path)
        {
            var verticesUnion = new UnionFind(numberOfVertices);
            foreach (var edge in path)
                verticesUnion.Union(edge.From, edge.To);

            var distinctTerminals = _terminalVertices.Select(terminal => verticesUnion.Find(terminal)).Distinct();
            return distinctTerminals.Count() == 1;
        }
    }

    public class MinimalPathWithEdgeVerificationBetweenTerminalsTestCase : MinimalPathBetweenTerminalsTestCase
    {
        public MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(MinimalPathBetweenTerminalsArgs args)
        : base(args)
        {
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            _result = ((Lab10)prototypeObject).Version2(_inputGraph, _terminalVertices);
        }
    }

    public class Lab10TestModule : TestModule
    {
        public override double ScoreResult()
        {
            return base.ScoreResult();
        }

        public override void PrepareTestSets()
        {
            var simpleTestCases = BuildSimpleTestCases();
            var largeTestCases = BuildLargeRandomTestCases();

            TestSets["version1SimpleTestCases "] = simpleTestCases.version1;
            TestSets["version1LargeTestCases "] = largeTestCases.version1;

            TestSets["version2SimpleTestCases "] = simpleTestCases.version2;
            TestSets["version2LargeTestCases "] = largeTestCases.version2;
        }

        private (TestSet version1, TestSet version2) BuildSimpleTestCases()
        {
            var version1TestSet = new TestSet(new Lab10(), "Part I, verify only length of minimum path");
            var version2TestSet = new TestSet(new Lab10(), "Part II, verify length and minimum path");
            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 3);
                graph.AddEdge(0, 1, 1);
                graph.AddEdge(1, 2, 4);
                graph.AddEdge(0, 2, 2);
                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph = graph,
                    TerminalVertices = new[] { 0, 1, 2 },
                    SolutionExists = true,
                    ExpectedSumOfEdgesWeights = 3.0,
                    VerifyPath = false,
                    TimeLimit = 1.0,
                    Description = "Simple graph",
                    ExpectedException = null
                };
                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 7);
                graph.AddEdge(0, 1, 3);
                graph.AddEdge(0, 6, 2);
                graph.AddEdge(1, 4, 4);
                graph.AddEdge(2, 6, 10);
                graph.AddEdge(2, 3, 5);
                graph.AddEdge(3, 6, 3);
                graph.AddEdge(3, 4, 5);
                graph.AddEdge(4, 5, 11);
                graph.AddEdge(4, 6, 1);
                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph = graph,
                    TerminalVertices = new[] { 1, 2, 5 },
                    SolutionExists = true,
                    ExpectedSumOfEdgesWeights = 24,
                    VerifyPath = false,
                    TimeLimit = 1.0,
                    Description = "Small irregular graph",
                    ExpectedException = null
                };
                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 8);
                graph.AddEdge(0, 3, 3);
                graph.AddEdge(1, 4, 3);
                graph.AddEdge(2, 3, 3);
                graph.AddEdge(3, 4, 10);
                graph.AddEdge(3, 6, 3);
                graph.AddEdge(4, 5, 3);
                graph.AddEdge(4, 7, 3);
                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph = graph,
                    TerminalVertices = new[] { 2, 5 },
                    SolutionExists = true,
                    ExpectedSumOfEdgesWeights = 16,
                    VerifyPath = false,
                    TimeLimit = 1.0,
                    Description = "Barbell",
                    ExpectedException = null
                };

                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 9);
                graph.AddEdge(0, 1, 1);
                graph.AddEdge(0, 3, 1);
                graph.AddEdge(1, 2, 1);
                graph.AddEdge(1, 4, 1);
                graph.AddEdge(2, 5, 1);
                graph.AddEdge(3, 4, 1);
                graph.AddEdge(3, 6, 1);
                graph.AddEdge(4, 5, 1);
                graph.AddEdge(4, 7, 1);
                graph.AddEdge(5, 8, 1);
                graph.AddEdge(6, 7, 1);
                graph.AddEdge(7, 8, 1);

                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph = graph,
                    TerminalVertices = new[] {2, 6},
                    SolutionExists = true,
                    ExpectedSumOfEdgesWeights = 4.0,
                    VerifyPath = false,
                    TimeLimit = 1.0,
                    Description = "3x3 dense graph, with weights equal to 1",
                    ExpectedException = null
                };

                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(true, 7);
                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph= graph,
                    TerminalVertices= new[] { 0, 1, 2, 4, 5 },
                    SolutionExists= false,
                    ExpectedSumOfEdgesWeights= 0,
                    VerifyPath= false,
                    TimeLimit= 1.0,
                    Description= "Graph with isolated vertices",
                    ExpectedException= null
                };
                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 21);
                graph.AddEdge(0, 1, 2);
                graph.AddEdge(1, 2, 2);
                graph.AddEdge(2, 3, 2);
                graph.AddEdge(3, 4, 2);
                graph.AddEdge(4, 5, 2);
                graph.AddEdge(5, 6, 2);
                graph.AddEdge(6, 7, 2);
                graph.AddEdge(7, 8, 2);
                graph.AddEdge(8, 9, 2);
                graph.AddEdge(9, 10, 2);
                graph.AddEdge(10, 11, 2);
                graph.AddEdge(11, 12, 2);
                graph.AddEdge(12, 13, 2);
                graph.AddEdge(13, 14, 2);
                graph.AddEdge(14, 15, 2);
                graph.AddEdge(15, 16, 2);
                graph.AddEdge(16, 17, 2);
                graph.AddEdge(17, 18, 2);
                graph.AddEdge(18, 19, 2);
                graph.AddEdge(19, 0, 2);
                graph.AddEdge(20, 4, 4);
                graph.AddEdge(20, 8, 4);
                graph.AddEdge(20, 14, 4);
                graph.AddEdge(20, 18, 4);


                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph= graph,
                    TerminalVertices= new[] {4, 8, 14, 18},
                    SolutionExists= true,
                    ExpectedSumOfEdgesWeights= 16,
                    VerifyPath= false,
                    TimeLimit= 1.0,
                    Description= "Circle graph",
                    ExpectedException= null
                };
                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 13);
                graph.AddEdge(0, 1, 5);
                graph.AddEdge(1, 3, 1);
                graph.AddEdge(1, 4, 4);
                graph.AddEdge(2, 4, 1);
                graph.AddEdge(2, 9, 3);
                graph.AddEdge(3, 4, 3);
                graph.AddEdge(3, 6, 1);
                graph.AddEdge(3, 7, 2);
                graph.AddEdge(4, 8, 1);
                graph.AddEdge(5, 6, 1);
                graph.AddEdge(5, 10, 1);
                graph.AddEdge(7, 11, 2);
                graph.AddEdge(8, 9, 1);
                graph.AddEdge(10, 11, 1);
                graph.AddEdge(11, 12, 2);
                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph= graph,
                    TerminalVertices= new[] { 1, 4, 6, 11 },
                    SolutionExists= true,
                    ExpectedSumOfEdgesWeights= 8,
                    VerifyPath= false,
                    TimeLimit= 5.0,
                    Description= "Irregular graph",
                    ExpectedException= null
                };
                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }
            
            return (version1TestSet, version2TestSet);
        }

        private (TestSet version1, TestSet version2) BuildLargeRandomTestCases()
        {
            var version1TestSet = new TestSet(new Lab10(), "Part I, large random test cases");
            var version2TestSet = new TestSet(new Lab10(), "Part II, large random test cases");

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 16);
                graph.AddEdge(0, 1, 1);
                graph.AddEdge(0, 4, 1);
                graph.AddEdge(1, 2, 1);
                graph.AddEdge(1, 5, 1);
                graph.AddEdge(2, 3, 1);
                graph.AddEdge(2, 6, 1);
                graph.AddEdge(3, 7, 1);
                graph.AddEdge(4, 5, 1);
                graph.AddEdge(4, 8, 1);
                graph.AddEdge(5, 6, 1);
                graph.AddEdge(5, 9, 1);
                graph.AddEdge(6, 7, 1);
                graph.AddEdge(6, 10, 1);
                graph.AddEdge(7, 11, 1);
                graph.AddEdge(8, 9, 1);
                graph.AddEdge(8, 12, 1);
                graph.AddEdge(9, 10, 1);
                graph.AddEdge(9, 13, 1);
                graph.AddEdge(10, 11, 1);
                graph.AddEdge(10, 14, 1);
                graph.AddEdge(11, 15, 1);
                graph.AddEdge(12, 13, 1);
                graph.AddEdge(13, 14, 1);
                graph.AddEdge(14, 15, 1);

                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph = graph,
                    TerminalVertices = new[] { 2, 4, 11, 13 },
                    SolutionExists = true,
                    ExpectedSumOfEdgesWeights = 7.0,
                    VerifyPath = false,
                    TimeLimit = 5.0,
                    Description = "4x4 dense graph, with weights equal to 1",
                    ExpectedException = null
                };

                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            {
                var graph = new AdjacencyListsGraph<AVLAdjacencyList>(false, 30);
                var numberOfColumns = 10;
                
                for (int i = 0; i < 30; i++)
                {
                    var row = i / numberOfColumns;
                    var column = i % numberOfColumns;
                    var lastVertexInRow = column == numberOfColumns-1;
                    var isFirstRow = row == 0;

                    var weight = isFirstRow ? 1 : 2;
                    var vertex = row * numberOfColumns + column;
                    var nextVertexInRow = vertex + 1;
                    var rowAboveVertex = (row - 1) * numberOfColumns + column;

                    if (!lastVertexInRow)
                        graph.AddEdge(vertex, nextVertexInRow, weight);
                    if(!isFirstRow)
                        graph.AddEdge(vertex, rowAboveVertex, weight);
                }
                
                var args = new MinimalPathBetweenTerminalsArgs
                {
                    InputGraph = graph,
                    TerminalVertices = new[] { 0, 9, 29 },
                    SolutionExists = true,
                    ExpectedSumOfEdgesWeights = 9 * 1.0 + 2 * 2.0,
                    VerifyPath = false,
                    TimeLimit = 70.0,
                    Description = "3x10, with weights equal to 1 or 2",
                    ExpectedException = null
                };

                version1TestSet.TestCases.Add(new MinimalPathBetweenTerminalsTestCase(args));
                args.VerifyPath = true;
                version2TestSet.TestCases.Add(new MinimalPathWithEdgeVerificationBetweenTerminalsTestCase(args));
            }

            return (version1TestSet, version2TestSet);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var verifyTimeLimits = false;

            var testModule = new Lab10TestModule();
            testModule.PrepareTestSets();
            foreach (var ts in testModule.TestSets)
                ts.Value.PerformTests(verbose: true, checkTimeLimit: verifyTimeLimits);
        }
    }
}
