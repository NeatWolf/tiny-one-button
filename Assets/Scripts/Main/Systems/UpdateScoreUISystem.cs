﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Tiny.Rendering;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
public class UpdateScoreUISystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<MaterialReferencesTag>();
        RequireSingletonForUpdate<GameData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var materialReferencesEntity = GetSingletonEntity<MaterialReferencesTag>();
        var gameData = GetSingleton<GameData>();

        var nBuffer = EntityManager.GetBuffer<UIMaterialReference>(materialReferencesEntity);
        var materials = nBuffer.ToNativeArray(Allocator.TempJob);

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities
            .WithAll<UpdateScoreTag, ActivatedTag>()
            .ForEach((Entity entity, ref MeshRenderer meshRenderer, in ScorePart scorePart) =>
            {
                int digit = (int)((float)gameData.score / scorePart.Divisor);
                meshRenderer.material = materials[digit % 10].materialEntity;
                ecb.RemoveComponent<ActivatedTag>(entity);
            }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();

        materials.Dispose();
        return default;
    }
}