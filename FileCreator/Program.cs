using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {

        long rowsInFile = 0L;
       // string filePath = @"C:\test\test200M.txt";
       string filePath = @".\test.txt";

        string[] vocabulary = {
            ".cat\n",
            ".hello world\n",
            ".programming\n",
            ".short\n",
            ".longer word\n",
            ".tiny\n",
            ".gigantic word\n",
            ".example phrase\n",
            ".a\n",
            ".this is a test\n",
            ".quick brown fox\n",
            ".lazy dog\n",
            ".jumping over\n",
            ".beautiful day\n",
            ".coding in C#\n",
            ".full stack developer\n",
            ".write clean code\n",
            ".optimize performance\n",
            ".debugging issues\n",
            ".software design\n",
            ".build great apps\n",
            ".refactor efficiently\n",
            ".design patterns\n",
            ".create APIs\n",
            ".user experience\n",
            ".database queries\n",
            ".RESTful services\n",
            ".async programming\n",
            ".handle exceptions\n",
            ".unit testing\n",
            ".functional programming\n",
            ".error handling\n",
            ".real-time processing\n",
            ".integrate systems\n",
            ".automate workflows\n",
            ".data structures\n",
            ".machine learning\n",
            ".artificial intelligence\n",
            ".neural networks\n",
            ".natural language processing\n",
            ".web development\n",
            ".frontend design\n",
            ".backend logic\n",
            ".cloud computing\n",
            ".DevOps pipelines\n",
            ".continuous integration\n",
            ".continuous deployment\n",
            ".test-driven development\n",
            ".dependency injection\n",
            ".secure authentication\n",
            ".encryption methods\n",
            ".responsive layouts\n",
            ".cross-platform apps\n",
            ".progressive web apps\n",
            ".microservices architecture\n",
            ".serverless functions\n",
            ".event-driven programming\n",
            ".graphical interfaces\n",
            ".desktop applications\n",
            ".version control\n",
            ".Git workflows\n",
            ".branching strategies\n",
            ".merge conflicts\n",
            ".code reviews\n",
            ".pull requests\n",
            ".collaborative development\n",
            ".agile methodologies\n",
            ".scrum meetings\n",
            ".Kanban boards\n",
            ".sprint planning\n",
            ".product backlog\n",
            ".retrospective sessions\n",
            ".stakeholder feedback\n",
            ".project management\n",
            ".time tracking\n",
            ".task prioritization\n",
            ".resource allocation\n",
            ".team collaboration\n",
            ".communication skills\n",
            ".technical documentation\n",
            ".API references\n",
            ".system requirements\n",
            ".architecture diagrams\n",
            ".ER diagrams\n",
            ".sequence diagrams\n",
            ".state machines\n",
            ".finite automata\n",
            ".graph algorithms\n",
            ".sorting algorithms\n",
            ".searching algorithms\n",
            ".big O notation\n",
            ".time complexity\n",
            ".space complexity\n",
            ".recursive functions\n",
            ".iterative solutions\n",
            ".divide and conquer\n",
            ".dynamic programming\n",
            ".greedy algorithms\n",
            ".graph traversal\n",
            ".breadth-first search\n",
            ".depth-first search\n",
            ".minimum spanning trees\n",
            ".Dijkstra's algorithm\n",
            ".A* search algorithm\n",
            ".hash tables\n",
            ".binary search trees\n",
            ".red-black trees\n",
            ".heap sort\n",
            ".quick sort\n",
            ".merge sort\n",
            ".bubble sort\n",
            ".insertion sort\n",
            ".selection sort\n",
            ".radix sort\n",
            ".bucket sort\n",
            ".counting sort\n",
            ".linear search\n",
            ".binary search\n",
            ".jump search\n",
            ".exponential search\n",
            ".fibonacci search\n",
            ".knapsack problem\n",
            ".travelling salesman\n",
            ".graph coloring\n",
            ".network flow\n",
            ".Ford-Fulkerson\n",
            ".Kruskal's algorithm\n",
            ".Prim's algorithm\n",
            ".connected components\n",
            ".topological sorting\n",
            ".strongly connected components\n",
            ".kosaraju's algorithm\n",
            ".tarjan's algorithm\n",
            ".dynamic connectivity\n",
            ".union-find\n",
            ".disjoint-set union\n",
            ".path compression\n",
            ".weighted union\n",
            ".trie data structure\n",
            ".suffix arrays\n",
            ".segment trees\n",
            ".fenwick trees\n",
            ".range queries\n",
            ".lazy propagation\n",
            ".priority queues\n",
            ".binary heaps\n",
            ".indexed heaps\n",
            ".max heaps\n",
            ".min heaps\n",
            ".graph databases\n",
            ".key-value stores\n",
            ".relational databases\n",
            ".NoSQL databases\n",
            ".MongoDB queries\n",
            ".SQL joins\n",
            ".foreign keys\n",
            ".primary keys\n",
            ".database normalization\n",
            ".first normal form\n",
            ".second normal form\n",
            ".third normal form\n",
            ".BCNF\n",
            ".ACID properties\n",
            ".CAP theorem\n",
            ".eventual consistency\n",
            ".distributed systems\n",
            ".load balancing\n",
            ".horizontal scaling\n",
            ".vertical scaling\n",
            ".containerization\n",
            ".Docker images\n",
            ".Kubernetes pods\n",
            ".orchestration tools\n",
            ".cluster management\n",
            ".service discovery\n",
            ".monitoring systems\n",
            ".logging frameworks\n",
            ".error tracking\n",
            ".metrics dashboards\n",
            ".Grafana setup\n",
            ".Prometheus queries\n",
            ".Elasticsearch index\n",
            ".Kibana visualizations\n",
            ".log aggregation\n",
            ".CI/CD pipelines\n",
            ".Jenkins jobs\n",
            ".GitLab runners\n",
            ".Azure DevOps\n",
            ".AWS Lambda\n",
            ".GCP functions\n",
            ".serverless deployment\n",
            ".infrastructure as code\n",
            ".Terraform scripts\n",
            ".CloudFormation templates\n"
        };

        //rowsInFile = 1000000000L;//20Gb
        //rowsInFile = 100000000L;//2Gb
        //rowsInFile = 10000000L;//0.2Gb
        Console.WriteLine("Enter the number of rows in the file (must be a positive number):");
        Console.WriteLine("1000000000 - 20Gb");
        Console.WriteLine("100000000 - 2Gb");
        Console.WriteLine("10000000 - 0.2Gb");
        while (rowsInFile == 0)
        {
            string? input = Console.ReadLine();
            if (!long.TryParse(input, out rowsInFile) || rowsInFile <= 0)
            {
                Console.WriteLine("Invalid input. Please enter a positive number.");
            }
        }
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var generator = new FileGenerator(vocabulary);
            //await generator.GenerateFileAsParallelAsync(filePath, rowsInFile);
            await generator.GenerateFileAsync(filePath, rowsInFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong: {ex.Message}");
            Console.ReadLine();
            return;
        }

        stopwatch.Stop();
        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"Created file: {fileInfo.FullName}");
        Console.WriteLine($"Rows written: {rowsInFile}");
       
        double fileSizeGb = fileInfo.Length / (1024.0 * 1024 * 1024);
        Console.WriteLine($"File size: {fileSizeGb:F2} GB");
        Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}");
        Console.ReadLine();
    }
}