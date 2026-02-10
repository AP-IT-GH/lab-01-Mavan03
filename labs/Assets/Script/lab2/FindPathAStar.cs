using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathMarker {

    public MapLocation location;
    public float G, H, F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject m, PathMarker p) {

        location = l;
        G = g;
        H = h;
        F = f;
        marker = m;
        parent = p;
    }

    public override bool Equals(object obj) {

        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        else
            return location.Equals(((PathMarker)obj).location);
    }

   
}

public class FindPathAStar : MonoBehaviour {

    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;
    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker startNode;
    PathMarker goalNode;
    PathMarker lastPos;
    bool done = false;
    bool hasStarted = false;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    void RemoveAllMarkers() {

        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");

        foreach (GameObject m in markers) Destroy(m);

        GameObject goal = GameObject.FindGameObjectWithTag("Goal");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
       // Destroy(goal);
        Destroy(player);
    }

    void BeginSearch() {

        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();

        for (int z = 1; z < maze.depth - 1; ++z) {
            for (int x = 1; x < maze.width - 1; ++x) {

                if (maze.map[x, z] != 1) {
                    locations.Add(new MapLocation(x, z));
                }
            }
        }
       // locations.Shuffle();

        Vector3 startLocation = new Vector3(1, 0.5f, 1);
        startNode = new PathMarker(new MapLocation(1, 1),
            0.0f, 0.0f, 0.0f, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 endLocation = new Vector3(Random.Range(5, 8), 0.5f, Random.Range(5, 8));
        goalNode = new PathMarker(new MapLocation((int)endLocation.x, (int)endLocation.z),
            0.0f, 0.0f, 0.0f, Instantiate(end, endLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();

        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode) {

        if (thisNode.Equals(goalNode)) {

              done = true;
            
            return;
        }

        foreach (MapLocation dir in maze.directions) {

            MapLocation neighbour = dir + thisNode.location;

            if (neighbour.x < 1 || neighbour.x > maze.width || neighbour.z < 1 || neighbour.z > maze.depth) continue;

            if (maze.map[neighbour.x, neighbour.z] == 1) continue;
            if (IsClosed(neighbour)) continue;

            float g = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float h = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float f = g + h;

            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0.0f, neighbour.z * maze.scale), Quaternion.identity);

            if (!UpdateMarker(neighbour, g, h, f, thisNode)) {

                open.Add(new PathMarker(neighbour, g, h, f, pathBlock, thisNode));
            }
        }
        open = open.OrderBy(p => p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);
        //pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt) {

        foreach (PathMarker p in open) {

            if (p.location.Equals(pos)) {

                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker) {

        foreach (PathMarker p in closed) {

            if (p.location.Equals(marker)) return true;
        }
        return false;
    }

    void Start() {

    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.P)) {

            BeginSearch();
            StartCoroutine(Searching());
        }
        

        if (hasStarted)
            if (Input.GetKeyDown(KeyCode.C)) Search(lastPos);
    }

    // The coroutine function
    bool searchingHasFinished = false;
    IEnumerator Searching()
    {
        Debug.Log("searching started!");

        while (!done && open.Count > 0)
        {
            // Perform some task
            Debug.Log("Coroutine is running...");
            Search(lastPos);
            // Wait for the next frame
            yield return new WaitForSeconds(0.05f);
        }
        if (done)
        {
            ReconstructPath(); // Maak het pad als we klaar zijn
            StartCoroutine(MovePlayer()); // Laat de speler lopen (Task 2.3)
        }
        else
        {
            Debug.Log("Geen pad gevonden!");
        }
    }
    IEnumerator MovePlayer()
    {
        // Zoek het spelersobject (zorg dat je Cube de tag "Player" heeft of sleep hem in de inspector)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Als er geen player tag is, gebruiken we de start marker als fallback voor visualisatie
        if (player == null && startNode != null) player = startNode.marker;

        foreach (PathMarker step in path)
        {
            // Beweeg de speler naar de locatie van de marker
            // We vermenigvuldigen met maze.scale om de juiste wereldpositie te krijgen
            Vector3 targetPos = new Vector3(step.location.x * maze.scale, 0.5f, step.location.z * maze.scale);

            if (player != null)
                player.transform.position = targetPos;

            // Wacht 1 seconde per stap zoals gevraagd in de opdracht 
            yield return new WaitForSeconds(1.0f);
        }
        Debug.Log("Speler heeft het doel bereikt!");
    }
    bool PathHasConstructed = false;
    List<PathMarker> path = new List<PathMarker>();
    void ReconstructPath()
    {
        path.Clear(); // Maak de lijst leeg voor de zekerheid
        PathMarker current = lastPos; // Begin bij het einde (het doel)

        // Zolang we nog niet bij de start zijn en er een parent is
        while (current.parent != null)
        {
            path.Insert(0, current); // Voeg toe aan het begin van de lijst
            current = current.parent; // Stap terug naar de ouder
        }
        // Voeg de start node ook toe
        path.Insert(0, current);

        PathHasConstructed = true;
        Debug.Log("Pad gevonden met " + path.Count + " stappen.");
    }

}
