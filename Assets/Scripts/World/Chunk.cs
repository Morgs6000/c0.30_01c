using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
    // Malha do chunk usada para renderizar os blocos
    private Mesh voxelMesh;

    // Listas de vértices, triângulos e coordenadas de textura da malha
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();

    // Enum com as faces de um bloco
    private enum VoxelSide { RIGHT, LEFT, TOP, BOTTOM, FRONT, BACK }

    // Índice do próximo vértice a ser adicionado à malha
    private int vertexIndex;

    // Tamanho do chunk em unidades de bloco
    public static Vector3 ChunkSizeInVoxels = new Vector3(16, 64, 16);

    // Dicionário que armazena as posições e os tipos de bloco presentes no chunk
    private VoxelType[,,] voxelMap = new VoxelType[(int)ChunkSizeInVoxels.x, (int)ChunkSizeInVoxels.y, (int)ChunkSizeInVoxels.z];

    // Tipo de bloco atual
    private VoxelType voxelType;

    // Lista de todos os chunks do mundo
    public static List<Chunk> chunkList = new List<Chunk>();

    private void Start() {        
        // Adiciona este chunk à lista de chunks
        chunkList.Add(this);

        // Gera os blocos do chunk
        ChunkGen();
    }

    private void Update() {
        // Atualizações do chunk podem ser feitas aqui (por exemplo, mudar a posição de um bloco)        
    }

    // Adiciona um bloco à voxel map e atualiza a malha do chunk
    public void SetBlock(Vector3 worldPos, VoxelType voxel) {
        // Calcula a posição local do bloco em relação ao chunk
        Vector3 localPos = worldPos - transform.position;

        int x = Mathf.FloorToInt(localPos.x);
        int y = Mathf.FloorToInt(localPos.y);
        int z = Mathf.FloorToInt(localPos.z);

        // Adiciona o bloco à voxel map
        voxelMap[x, y, z] = voxel;

        // Atualiza a malha do chunk para refletir o novo bloco
        ChunkRenderer();
    }

    // Retorna o chunk que contém a posição dada, se houver
    public static Chunk GetChunk(Vector3 pos) {
        for(int i = 0; i < chunkList.Count; i++) {            
            // Calcula a posição do chunk atual
            Vector3 chunkPos = chunkList[i].transform.position;

            // Verifica se a posição dada está dentro dos limites do chunk
            if(
                pos.x < chunkPos.x || pos.x >= chunkPos.x + ChunkSizeInVoxels.x || 
                pos.y < chunkPos.y || pos.y >= chunkPos.y + ChunkSizeInVoxels.y || 
                pos.z < chunkPos.z || pos.z >= chunkPos.z + ChunkSizeInVoxels.z
            ) {
                // A posição não está neste chunk, passa para o próximo
                continue;
            }

            // A posição está neste chunk, retorna o chunk
            return chunkList[i];
        }

        // Nenhum chunk contém a posição dada
        return null;
    }

    private void TreeGen(Vector3 offset) {
        int x = (int)offset.x;
        int y = (int)offset.y;
        int z = (int)offset.z;

        float _x = x + transform.position.x;
        float _y = y + transform.position.y;
        float _z = z + transform.position.z;

        _x += (World.WorldSizeInVoxels.x);
        //_y += (World.WorldSizeInVoxels.y);
        _z += (World.WorldSizeInVoxels.z);
        
        if(
            Random.Range(0, 100) < 1 &&
            _y == Noise.Perlin(_x, _z) + 1
        ) {            
            //int leavesWidth = 5;
            int leavesHeight = Random.Range(3, 5);

            int iter = 0;
            
            for(int yL = y + 0; yL < y + leavesHeight; yL++) {
                for(int xL = x - 2 + iter / 2; xL <  x + 3 - iter / 2; xL++) {                
                    for(int zL = z - 2 + iter / 2; zL <  z + 3 - iter / 2; zL++) {
                        if(
                            xL >= 0 && xL < ChunkSizeInVoxels.x &&
                            yL >= 0 && yL < ChunkSizeInVoxels.y &&
                            zL >= 0 && zL < ChunkSizeInVoxels.z
                        ) {
                            voxelMap[xL, yL + 3, zL] = VoxelType.oak_leaves;
                        } 
                    }                   
                }

                iter++;                
            }

            int treeHeight = Random.Range(3, 5);

            for(int i = 0; i < treeHeight; i++) {
                if(y + i < ChunkSizeInVoxels.y) {
                    voxelMap[x, y + i, z] = VoxelType.oak_log;
                }                
            }
        }
    }

    private void VoxelLayers(Vector3 offset) {
        int x = (int)offset.x;
        int y = (int)offset.y;
        int z = (int)offset.z;

        float _x = x + transform.position.x;
        float _y = y + transform.position.y;
        float _z = z + transform.position.z;

        _x += (World.WorldSizeInVoxels.x);
        //_y += (World.WorldSizeInVoxels.y);
        _z += (World.WorldSizeInVoxels.z);

        if(
            _y == 0 ||
            _y <= 4 && 
            Random.Range(0, 100) < 50            
        ) {
            voxelMap[x, y, z] = VoxelType.bedrock;
        }
        else if(
            _y >= 6 &&
            Noise.Perlin3D(_x * 0.05f, _y * 0.05f, _z * 0.05f) >= 0.5f &&
            _y < Noise.Perlin(_x, _z) - 5
        ) {
            voxelMap[x, y, z] = VoxelType.air;
        }
        else if(_y < Noise.Perlin(_x, _z) - 4) {
            voxelMap[x, y, z] = VoxelType.stone;
        }
        else if(_y < Noise.Perlin(_x, _z)) {
            voxelMap[x, y, z] = VoxelType.dirt;
        }
        else if(_y == Noise.Perlin(_x, _z)) {
            voxelMap[x, y, z] = VoxelType.grass_block;
        }
        else {
            voxelMap[x, y, z] = VoxelType.air;
        }
    }
    
    /*
    // Gera as camadas de blocos do chunk de acordo com sua posição
    private VoxelType VoxelLayers(Vector3 offset) {
        // Adiciona um bloco de pedra abaixo da superfície
        if(offset.y < Noise.Perlin(offset.x, offset.z) - 4) {
            return VoxelType.stone;
        }
        // Adiciona a camada de terra
        else if(offset.y < Noise.Perlin(offset.x, offset.z)) {
            return VoxelType.dirt;
        }
        // Adiciona um bloco de grama na superfície
        else if(offset.y == Noise.Perlin(offset.x, offset.z)) {
            return VoxelType.grass_block;
        }
        // Adiciona um bloco de ar acima da superfície
        else {
            return VoxelType.air;
        }
    }
    */

    // Gera todos os blocos do chunk
    private void ChunkGen() {
        for(int x = 0; x < ChunkSizeInVoxels.x; x++) {
            for(int y = 0; y < ChunkSizeInVoxels.y; y++) {
                for(int z = 0; z < ChunkSizeInVoxels.z; z++) {
                    //voxelMap[x, y, z] = VoxelLayers(new Vector3(x, y, z) + transform.position);
                    VoxelLayers(new Vector3(x, y, z));
                    TreeGen(new Vector3(x, y, z));
                }
            }
        }

        // Atualiza a malha do chunk para refletir todos os blocos
        ChunkRenderer();
    }

    // Atualiza a malha do chunk para refletir a voxel map atual
    // É chamada sempre que um bloco é adicionado ou removido do chunk.
    private void ChunkRenderer() {
        // Cria uma nova malha
        voxelMesh = new Mesh();
        voxelMesh.name = "Chunk";

        // Limpa as listas de vértices, triângulos e coordenadas de textura
        vertices.Clear();
        triangles.Clear();
        uv.Clear();

        // Reseta o índice de vértices
        vertexIndex = 0;

        // Percorre os voxels do chunk
        for(int x = 0; x < ChunkSizeInVoxels.x; x++) {
            for(int y = 0; y < ChunkSizeInVoxels.y; y++) {
                for(int z = 0; z < ChunkSizeInVoxels.z; z++) {
                    // Se o voxel atual não for ar, adiciona suas faces à malha
                    if(voxelMap[x, y, z] != VoxelType.air) {
                        BlockGen(new Vector3(x, y, z));
                    }
                }
            }
        }

        MeshRenderer();
    }

    private void MeshRenderer() {
        // Atribui as listas de vértices, triângulos e coordenadas de textura à malha
        voxelMesh.vertices = vertices.ToArray();
        voxelMesh.triangles = triangles.ToArray();
        voxelMesh.uv = uv.ToArray();

        voxelMesh.RecalculateNormals();
        voxelMesh.Optimize();

        GetComponent<MeshCollider>().sharedMesh = voxelMesh;
        GetComponent<MeshFilter>().mesh = voxelMesh;
    }

    private bool HasSolidNeighbor(Vector3 offset) {
        int x = (int)offset.x;
        int y = (int)offset.y;
        int z = (int)offset.z;
        
        if(
            x < 0 || x > ChunkSizeInVoxels.x - 1 ||
            y < 0 || y > ChunkSizeInVoxels.y - 1 ||
            z < 0 || z > ChunkSizeInVoxels.z - 1
        ) {
            return false;
        }
        else if(voxelMap[x, y, z] == VoxelType.air) {
            return false;
        }
        else {
            return true;
        }        
    }

    private void BlockGen(Vector3 offset) {
        int x = (int)offset.x;
        int y = (int)offset.y;
        int z = (int)offset.z;
        
        voxelType = voxelMap[x, y, z];

        if(!HasSolidNeighbor(new Vector3(1, 0, 0) + offset)) {
            VerticesAdd(VoxelSide.RIGHT, offset);
        }
        if(!HasSolidNeighbor(new Vector3(-1, 0, 0) + offset)) {
            VerticesAdd(VoxelSide.LEFT, offset);
        }
        if(!HasSolidNeighbor(new Vector3(0, 1, 0) + offset)) {
            VerticesAdd(VoxelSide.TOP, offset);
        }
        if(!HasSolidNeighbor(new Vector3(0, -1, 0) + offset)) {
            VerticesAdd(VoxelSide.BOTTOM, offset);
        }
        if(!HasSolidNeighbor(new Vector3(0, 0, 1) + offset)) {
            VerticesAdd(VoxelSide.FRONT, offset);
        }
        if(!HasSolidNeighbor(new Vector3(0, 0, -1) + offset)) {
            VerticesAdd(VoxelSide.BACK, offset);
        }
    }

    private void VerticesAdd(VoxelSide side, Vector3 offset) {
        switch(side) {
            case VoxelSide.RIGHT: {
                vertices.Add(new Vector3(1, 0, 0) + offset);
                vertices.Add(new Vector3(1, 1, 0) + offset);
                vertices.Add(new Vector3(1, 1, 1) + offset);
                vertices.Add(new Vector3(1, 0, 1) + offset);

                break;
            }
            case VoxelSide.LEFT: {
                vertices.Add(new Vector3(0, 0, 1) + offset);
                vertices.Add(new Vector3(0, 1, 1) + offset);
                vertices.Add(new Vector3(0, 1, 0) + offset);
                vertices.Add(new Vector3(0, 0, 0) + offset);

                break;
            }
            case VoxelSide.TOP: {
                vertices.Add(new Vector3(0, 1, 0) + offset);
                vertices.Add(new Vector3(0, 1, 1) + offset);
                vertices.Add(new Vector3(1, 1, 1) + offset);
                vertices.Add(new Vector3(1, 1, 0) + offset);

                break;
            }
            case VoxelSide.BOTTOM: {
                vertices.Add(new Vector3(1, 0, 0) + offset);
                vertices.Add(new Vector3(1, 0, 1) + offset);
                vertices.Add(new Vector3(0, 0, 1) + offset);
                vertices.Add(new Vector3(0, 0, 0) + offset);

                break;
            }
            case VoxelSide.FRONT: {
                vertices.Add(new Vector3(1, 0, 1) + offset);
                vertices.Add(new Vector3(1, 1, 1) + offset);
                vertices.Add(new Vector3(0, 1, 1) + offset);
                vertices.Add(new Vector3(0, 0, 1) + offset);

                break;
            }
            case VoxelSide.BACK: {
                vertices.Add(new Vector3(0, 0, 0) + offset);
                vertices.Add(new Vector3(0, 1, 0) + offset);
                vertices.Add(new Vector3(1, 1, 0) + offset);
                vertices.Add(new Vector3(1, 0, 0) + offset);

                break;
            }
        }

        TrianglesAdd();

        UVsPos(side);
    }

    private void TrianglesAdd() {
        // Primeiro Triangulo
        triangles.Add(0 + vertexIndex);
        triangles.Add(1 + vertexIndex);
        triangles.Add(2 + vertexIndex);

        // Segundo Triangulo
        triangles.Add(0 + vertexIndex);
        triangles.Add(2 + vertexIndex);
        triangles.Add(3 + vertexIndex);

        vertexIndex += 4;
    }

    private void UVsAdd(Vector2 textureCoordinate) {
        Vector2 offset = new Vector2(
            0, 
            0
        );

        Vector2 textureSizeInTiles = new Vector2(
            16 + offset.x,
            16 + offset.y
        );
        
        float x = textureCoordinate.x + offset.x;
        float y = textureCoordinate.y + offset.y;

        float _x = 1.0f / textureSizeInTiles.x;
        float _y = 1.0f / textureSizeInTiles.y;

        y = (textureSizeInTiles.y - 1) - y;

        x *= _x;
        y *= _y;

        uv.Add(new Vector2(x, y));
        uv.Add(new Vector2(x, y + _y));
        uv.Add(new Vector2(x + _x, y + _y));
        uv.Add(new Vector2(x + _x, y));
    }

    private void UVsPos(VoxelSide side) {
        // Pre-Classic | rd-132211
        
        // STONE
        if(voxelType == VoxelType.stone) {
            UVsAdd(new Vector2(1, 0));
        }

        // GRASS BLOCK
        if(voxelType == VoxelType.grass_block) {
            if(side == VoxelSide.TOP) {
                UVsAdd(new Vector2(0, 0));
                return;
            }
            if(side == VoxelSide.BOTTOM) {
                UVsAdd(new Vector2(2, 0));
                return;
            }            
            UVsAdd(new Vector2(3, 0));
        }

        // Pre-Classic | rd-160052
        
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

        // Pre-Classic | rd-161348

        // OAK SAPLING
        if(voxelType == VoxelType.oak_sapling) {
            UVsAdd(new Vector2(15, 0));
        }

        // Classic | Early Classic | 0.0.12a

        // BEDROCK
        if(voxelType == VoxelType.bedrock) {
            UVsAdd(new Vector2(1, 1));
        }

        // WATER
        if(voxelType == VoxelType.water) {
            UVsAdd(new Vector2(14, 0));
        }

        // LAVA
        if(voxelType == VoxelType.lava) {
            UVsAdd(new Vector2(14, 1));
        }

        // Classic | Early Classic | 0.0.14a

        // SAND
        if(voxelType == VoxelType.sand) {
            UVsAdd(new Vector2(2, 1));
        }

        // GRAVEL
        if(voxelType == VoxelType.gravel) {
            UVsAdd(new Vector2(3, 1));
        }

        // COAL ORE
        if(voxelType == VoxelType.coal_ore) {
            UVsAdd(new Vector2(2, 2));
        }

        // IRON ORE
        if(voxelType == VoxelType.iron_ore) {
            UVsAdd(new Vector2(1, 2));
        }

        // GOLD ORE
        if(voxelType == VoxelType.gold_ore) {
            UVsAdd(new Vector2(0, 2));
        }

        // OAK LOG
        if(voxelType == VoxelType.oak_log) {
            if(side == VoxelSide.TOP || side == VoxelSide.BOTTOM) {
                UVsAdd(new Vector2(5, 1));
                return;
            }            
            UVsAdd(new Vector2(4, 1));
        }

        // OAK LEAVES
        if(voxelType == VoxelType.oak_leaves) {
            UVsAdd(new Vector2(6, 1));
        }

        // Classic | Multiplayer Test | 0.0.19a

        // SPONGE
        if(voxelType == VoxelType.sponge) {
            UVsAdd(new Vector2(0, 3));
        }

        // GLASS
        if(voxelType == VoxelType.glass) {
            UVsAdd(new Vector2(1, 3));
        }

        // Classic | Multiplayer Test | 0.0.20a

        // WITHE CLOTH
        if(voxelType == VoxelType.withe_cloth) {
            UVsAdd(new Vector2(15, 4));
        }

        // LIGHT GRAY CLOTH
        if(voxelType == VoxelType.light_gray_cloth) {
            UVsAdd(new Vector2(14, 4));
        }

        // DARK GRAY CLOTH
        if(voxelType == VoxelType.dark_gray_cloth) {
            UVsAdd(new Vector2(13, 4));
        }

        // RED CLOTH
        if(voxelType == VoxelType.red_cloth) {
            UVsAdd(new Vector2(0, 4));
        }

        // ORANNGE CLOTH
        if(voxelType == VoxelType.orange_cloth) {
            UVsAdd(new Vector2(1, 4));
        }

        // YELLOW CLOTH
        if(voxelType == VoxelType.yellow_cloth) {
            UVsAdd(new Vector2(2, 4));
        }

        // CHARTREUSE CLOTH
        if(voxelType == VoxelType.chartreuse_cloth) {
            UVsAdd(new Vector2(3, 4));
        }

        // GREEN CLOTH
        if(voxelType == VoxelType.green_cloth) {
            UVsAdd(new Vector2(4, 4));
        }

        // SPRING GREEN CLOTH
        if(voxelType == VoxelType.spring_green_cloth) {
            UVsAdd(new Vector2(5, 4));
        }

        // CYAN CLOTH
        if(voxelType == VoxelType.cyan_cloth) {
            UVsAdd(new Vector2(6, 4));
        }

        // CAPRI CLOTH
        if(voxelType == VoxelType.capri_cloth) {
            UVsAdd(new Vector2(7, 4));
        }

        // ULTRAMARINE CLOTH
        if(voxelType == VoxelType.ultramarine_cloth) {
            UVsAdd(new Vector2(8, 4));
        }

        // VIOLET CLOTH
        if(voxelType == VoxelType.violet_cloth) {
            UVsAdd(new Vector2(9, 4));
        }

        // PURPLE CLOTH
        if(voxelType == VoxelType.purple_cloth) {
            UVsAdd(new Vector2(10, 4));
        }

        // MAGENTA CLOTH
        if(voxelType == VoxelType.magenta_cloth) {
            UVsAdd(new Vector2(11, 4));
        }

        // ROSE CLOTH
        if(voxelType == VoxelType.rose_cloth) {
            UVsAdd(new Vector2(12, 4));
        }

        // BLOCK OF GOLD
        if(voxelType == VoxelType.block_of_gold) {
            if(side == VoxelSide.TOP) {
                UVsAdd(new Vector2(8, 1));
                return;
            }
            if(side == VoxelSide.BOTTOM) {
                UVsAdd(new Vector2(8, 3));
                return;
            }            
            UVsAdd(new Vector2(8, 2));
        }

        // DANDELION
        if(voxelType == VoxelType.dandelion) {
            UVsAdd(new Vector2(13, 0));
        }

        // ROSE
        if(voxelType == VoxelType.rose) {
            UVsAdd(new Vector2(12, 0));
        }

        // RED MUSHROOM
        if(voxelType == VoxelType.red_mushroom) {
            UVsAdd(new Vector2(12, 1));
        }

        // BROWN MUSHROOM
        if(voxelType == VoxelType.brown_mushroom) {
            UVsAdd(new Vector2(13, 1));
        }

        // Classic | Survival Test | 0.26 SURVIVAL TEST

        // SMOOTH STONE SLAB
        // DOUBLE SMOOTH STONE SLAB
        if(voxelType == VoxelType.double_smooth_stone_slab) {
            if(side == VoxelSide.TOP || side == VoxelSide.BOTTOM) {
                UVsAdd(new Vector2(6, 0));
                return;
            }            
            UVsAdd(new Vector2(5, 0));
        }

        // BLOCK OF IRON
        if(voxelType == VoxelType.block_of_iron) {
            if(side == VoxelSide.TOP) {
                UVsAdd(new Vector2(7, 1));
                return;
            }
            if(side == VoxelSide.BOTTOM) {
                UVsAdd(new Vector2(7, 3));
                return;
            }            
            UVsAdd(new Vector2(7, 2));
        }

        // TNT
        if(voxelType == VoxelType.tnt) {
            if(side == VoxelSide.TOP) {
                UVsAdd(new Vector2(9, 0));
                return;
            }
            if(side == VoxelSide.BOTTOM) {
                UVsAdd(new Vector2(10, 0));
                return;
            }            
            UVsAdd(new Vector2(8, 0));
        }

        // MOSSY COBBLESTONE
        if(voxelType == VoxelType.mossy_cobblestone) {
            UVsAdd(new Vector2(4, 2));
        }

        // BRICKS
        if(voxelType == VoxelType.bricks) {
            UVsAdd(new Vector2(7, 0));
        }

        // BOOKSHELF
        if(voxelType == VoxelType.bookshelf) {
            if(side == VoxelSide.TOP || side == VoxelSide.BOTTOM) {
                UVsAdd(new Vector2(4, 0));
                return;
            }            
            UVsAdd(new Vector2(3, 2));
        }

        // Classic | Late Classic | 0.28

        // OBSIDIAN
        if(voxelType == VoxelType.obsidian) {
            UVsAdd(new Vector2(5, 2));
        }
    }
}
