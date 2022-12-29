using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoxelType {
    // Pre-Classic | rd-132211
    air,
    stone,
    grass_block,

    // Pre-Classic | rd-160052
    dirt,
    cobblestone,
    oak_planks,

    // Pre-Classic | rd-161348
    oak_sapling,

    // Classic | Early Classic | 0.0.12a
    bedrock,
    water,
    lava,

    // Classic | Early Classic | 0.0.14a
    sand,
    gravel,
    coal_ore,
    iron_ore,
    gold_ore,
    oak_log,
    oak_leaves,
    //oak_leaves_fast,

    // Classic | Multiplayer Test | 0.0.19a
    sponge,
    glass,

    // Classic | Multiplayer Test | 0.0.20a
    withe_cloth,
    light_gray_cloth,
    dark_gray_cloth,
    red_cloth,
    orange_cloth,
    yellow_cloth,
    chartreuse_cloth,
    green_cloth,
    spring_green_cloth,
    cyan_cloth,
    capri_cloth,
    ultramarine_cloth,
    violet_cloth,
    purple_cloth,
    magenta_cloth,
    rose_cloth,
    block_of_gold,
    dandelion,
    rose,
    red_mushroom,
    brown_mushroom,

    // Classic | Survival Test | 0.26 SURVIVAL TEST
    smooth_stone_slab,
    double_smooth_stone_slab,
    block_of_iron,
    tnt,
    mossy_cobblestone,
    bricks,
    bookshelf,

    // Classic | Late Classic | 0.28
    obsidian,
}
