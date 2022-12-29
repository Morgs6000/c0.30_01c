using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este script é um componente que adiciona um efeito de destaque a um objeto quando um raio é lançado da câmera e atinge um objeto na cena.
public class Highlight : MonoBehaviour {
    // O objeto de destaque a ser ativado e posicionado quando o raio é atingido.
    [SerializeField] private GameObject highlight;
        
    // Referências aos componentes MeshFilter e MeshRenderer do objeto de destaque.
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    // A malha a ser usada pelo objeto de destaque.
    private Mesh highlighMesh;

    // As listas de vértices e triângulos da malha.
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();

    // O material a ser usado pelo objeto de destaque.
    [SerializeField] private Material material;
    [SerializeField] private Material terrainMaterial;

    // Um enumerador que representa os lados do objeto de destaque.
    private enum HighlighSide { RIGHT, LEFT, TOP, BOTTOM, FRONT, BACK }

    // O número total de vértices na malha.
    private int vertexIndex;

    private VoxelType voxelType;

    [SerializeField] private SelectedBlock selectedBlock;

    // A transformação da câmera a ser usada para lançar o raio.
    [SerializeField] private Transform cam;
    // A distância máxima em que o raio pode atingir um objeto.
    private float rangeHit = 5.0f;
    // A máscara de camadas a ser usada para determinar quais objetos o raio pode atingir.
    [SerializeField] private LayerMask groundMask;

    private bool destroyVoxels;
    
    // Inicializa o objeto de destaque, criando sua malha e adicionando os componentes MeshFilter e MeshRenderer.
    private void Start() {
        // Adicione os componentes MeshFilter e MeshRenderer ao objeto de destaque.
        meshFilter = (MeshFilter)highlight.AddComponent(typeof(MeshFilter));
        meshRenderer = (MeshRenderer)highlight.AddComponent(typeof(MeshRenderer));

        // Crie a malha
        highlighMesh = new Mesh();
        highlighMesh.name = "Highlight";

        destroyVoxels = true;
    }

    // Atualiza o objeto de destaque a cada quadro.
    private void Update() {
        voxelType = selectedBlock.GetCurrentItem();  

        // Limpa as listas de vértices, triângulos e coordenadas de textura
        vertices.Clear();
        triangles.Clear();
        uv.Clear();

        // Reseta o índice de vértices
        vertexIndex = 0;

        // Gere a malha para o objeto de destaque.
        HighlighGen();
        // Adicione a malha ao MeshFilter e defina o material do MeshRenderer.
        MeshRenderer();

        // Atualize a posição do objeto de destaque de acordo com o resultado do raio lançado da câmera.
        HighlightUpdates();

        /*
        // Atualize a transparência do material do objeto de destaque para criar um efeito de piscar.
        ColorUpdate();
        */

        if(Input.GetMouseButtonDown(1)) {
            destroyVoxels = !destroyVoxels;
        }
    }

    // Atualiza a transparência do material do objeto de destaque para criar um efeito de piscar.
    private void ColorUpdate() {
        // Defina a cor inicial com uma transparência de 0,5.
        Color colorA = material.color;
        colorA.a = 0.5f;

        // Defina a cor final com uma transparência de 0.
        Color colorB = material.color;
        colorB.a = 0.0f;

        // A velocidade do efeito de piscar.
        float speed = 2;

        // Defina o material do objeto de destaque e altere sua cor usando o método Lerp do Unity para criar o efeito de piscar.
        meshRenderer.material = material;
        meshRenderer.material.color = Color.Lerp(colorA, colorB, Mathf.PingPong(Time.time * speed, 1));
    }

    // Atualiza a transparência do material do objeto de destaque para criar um efeito de piscar.
    private void ColorUpdate2() {
        // Defina a cor inicial com uma transparência de 0,5.
        Color colorA = terrainMaterial.color;
        colorA.a = 0.5f;

        // Defina a cor final com uma transparência de 0.
        Color colorB = terrainMaterial.color;
        colorB.a = 0.0f;

        // A velocidade do efeito de piscar.
        float speed = 2;

        // Defina o material do objeto de destaque e altere sua cor usando o método Lerp do Unity para criar o efeito de piscar.
        meshRenderer.material = terrainMaterial;
        meshRenderer.material.color = Color.Lerp(colorA, colorB, Mathf.PingPong(Time.time * speed, 1));
    }

    // Atualiza a posição do objeto de destaque de acordo com o resultado do raio lançado da câmera.
    private void HighlightUpdates() {
        RaycastHit hit;

        // Lança um raio a partir da câmera e armazena o resultado em hit.
        if(Physics.Raycast(cam.position, cam.forward, out hit, rangeHit, groundMask)) {
            // Ative o objeto de destaque.
            highlight.SetActive(true);

            if(destroyVoxels) {
                // Calcula a posição do objeto atingido pelo raio.
                Vector3 pointPos = hit.point - hit.normal / 2;
                
                // Posicione o objeto de destaque na posição do objeto atingido, arredondando os valores de posição para inteiros.
                highlight.transform.position = new Vector3(
                    Mathf.FloorToInt(pointPos.x),
                    Mathf.FloorToInt(pointPos.y),
                    Mathf.FloorToInt(pointPos.z)
                );
                
                //*
                // Atualize a transparência do material do objeto de destaque para criar um efeito de piscar.
                ColorUpdate();
                //*/
            }
            if(!destroyVoxels) {
                // Calcula a posição do objeto atingido pelo raio.
                Vector3 pointPos = hit.point + hit.normal / 2;
                
                // Posicione o objeto de destaque na posição do objeto atingido, arredondando os valores de posição para inteiros.
                highlight.transform.position = new Vector3(
                    Mathf.FloorToInt(pointPos.x),
                    Mathf.FloorToInt(pointPos.y),
                    Mathf.FloorToInt(pointPos.z)
                );
                
                //*
                // Atualize a transparência do material do objeto de destaque para criar um efeito de piscar.
                ColorUpdate2();
                //*/
            }
        }
        else {
            // Desative o objeto de destaque se o raio não atingir nenhum objeto.
            highlight.SetActive(false);          
        }
    }

    // Adicione a malha ao MeshFilter e defina o material do MeshRenderer.
    private void MeshRenderer() {
        // Defina os vértices e triângulos da malha.
        highlighMesh.vertices = vertices.ToArray();
        highlighMesh.triangles = triangles.ToArray();
        highlighMesh.uv = uv.ToArray();

        // Recalcule as normais da malha e otimize-a.
        highlighMesh.RecalculateNormals();
        highlighMesh.Optimize();

        // Adicione a malha ao MeshFilter do seu GameObject
        meshFilter.mesh = highlighMesh;
    }

    // Gere a malha para o objeto de destaque.
    private void HighlighGen() {
        // Adicione os vértices para cada lado do objeto de destaque.
        VerticesAdd(HighlighSide.RIGHT);
        VerticesAdd(HighlighSide.LEFT);
        VerticesAdd(HighlighSide.TOP);
        VerticesAdd(HighlighSide.BOTTOM);
        VerticesAdd(HighlighSide.FRONT);
        VerticesAdd(HighlighSide.BACK);
    }

    // Adicione os vértices da malha para o lado especificado do objeto de destaque.
    private void VerticesAdd(HighlighSide side) {
        switch(side) {
            case HighlighSide.RIGHT: {
                // Adicione os vértices para o lado leste do objeto de destaque.
                vertices.Add(new Vector3(1, 0, 0));
                vertices.Add(new Vector3(1, 1, 0));
                vertices.Add(new Vector3(1, 1, 1));
                vertices.Add(new Vector3(1, 0, 1));

                break;
            }
            case HighlighSide.LEFT: {
                // Adicione os vértices para o lado oeste do objeto de destaque.
                vertices.Add(new Vector3(0, 0, 1));
                vertices.Add(new Vector3(0, 1, 1));
                vertices.Add(new Vector3(0, 1, 0));
                vertices.Add(new Vector3(0, 0, 0));

                break;
            }
            case HighlighSide.TOP: {
                // Adicione os vértices para o topo do objeto de destaque.
                vertices.Add(new Vector3(0, 1, 0));
                vertices.Add(new Vector3(0, 1, 1));
                vertices.Add(new Vector3(1, 1, 1));
                vertices.Add(new Vector3(1, 1, 0));

                break;
            }
            case HighlighSide.BOTTOM: {
                // Adicione os vértices para o fundo do objeto de destaque.
                vertices.Add(new Vector3(1, 0, 0));
                vertices.Add(new Vector3(1, 0, 1));
                vertices.Add(new Vector3(0, 0, 1));
                vertices.Add(new Vector3(0, 0, 0));

                break;
            }
            case HighlighSide.FRONT: {
                // Adicione os vértices para o lado norte do objeto de destaque.
                vertices.Add(new Vector3(1, 0, 1));
                vertices.Add(new Vector3(1, 1, 1));
                vertices.Add(new Vector3(0, 1, 1));
                vertices.Add(new Vector3(0, 0, 1));

                break;
            }
            case HighlighSide.BACK: {
                // Adicione os vértices para o lado sul do objeto de destaque.
                vertices.Add(new Vector3(0, 0, 0));
                vertices.Add(new Vector3(0, 1, 0));
                vertices.Add(new Vector3(1, 1, 0));
                vertices.Add(new Vector3(1, 0, 0));

                break;
            }
        }

        // Adicione os triângulos para o lado atual do objeto de destaque.
        TrianglesAdd();

        UVsPos(side);
    }

    // Adicone os Triangulos dos Vertices para renderizar a face
    private void TrianglesAdd() {
        // Primeiro Tiangulo
        triangles.Add(0 + vertexIndex);
        triangles.Add(1 + vertexIndex);
        triangles.Add(2 + vertexIndex);

        // Segundo Triangulo
        triangles.Add(0 + vertexIndex);
        triangles.Add(2 + vertexIndex);
        triangles.Add(3 + vertexIndex);

        vertexIndex += 4;
    }

    // Adicione as UVs dos Vertices para renderizar a textura
    private void UVsAdd(Vector2 textureCoordinate) {
        Vector2 offset = new Vector2(
            0, 
            0
        );

        Vector2 textureSize = new Vector2(
            16 + offset.x,
            16 + offset.y
        );
        
        float x = textureCoordinate.x + offset.x;
        float y = textureCoordinate.y + offset.y;

        float _x = 1.0f / textureSize.x;
        float _y = 1.0f / textureSize.y;

        y = (textureSize.y - 1) - y;

        x *= _x;
        y *= _y;

        uv.Add(new Vector2(x, y));
        uv.Add(new Vector2(x, y + _y));
        uv.Add(new Vector2(x + _x, y + _y));
        uv.Add(new Vector2(x + _x, y));
    }

    // Pegue a posição da UV no Texture Atlas
    private void UVsPos(HighlighSide side) {
        // Pre-Classic | rd-132211
        
        // STONE
        if(voxelType == VoxelType.stone) {
            UVsAdd(new Vector2(1, 0));
        }

        // GRASS BLOCK
        if(voxelType == VoxelType.grass_block) {
            if(side == HighlighSide.TOP) {
                UVsAdd(new Vector2(0, 0));
                return;
            }
            if(side == HighlighSide.TOP) {
                UVsAdd(new Vector2(2, 0));
                return;
            }            
            UVsAdd(new Vector2(3, 0));
        }
        
        // DIRT
        if(voxelType == VoxelType.dirt) {
            UVsAdd(new Vector2(2, 0));
        }
        
        // COBBLESTONE
        if(voxelType == VoxelType.cobblestone) {
            UVsAdd(new Vector2(0, 1));
        }
        
        // OAK PLANKS
        if(voxelType == VoxelType.oak_planks) {
            UVsAdd(new Vector2(4, 0));
        }
    }
}
