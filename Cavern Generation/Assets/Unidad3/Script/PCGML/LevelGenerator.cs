using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [TextArea]
    public string inputExample;

    public int N = 2;
    public int outputLenght = 20;
    public int width = 20;

    public LevelRenderer renderer;

    private MarkovModel model;

    void Start()
    {
        GenerateLevelMarkov();
    }

    public void GenerateLevelMarkov()
    {

        model = new MarkovModel(N);
        model.Train(inputExample);

        string generated = model.Generate(outputLenght);
        Debug.Log("Resultado Esperado: " + generated);

        renderer.Render(generated, width);
    }
}
