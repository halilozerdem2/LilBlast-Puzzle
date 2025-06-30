using System.Collections.Generic;
using UnityEngine;

public class ColorBombBlock : Block
{
    public int targetColorType ;// hangi rengi yok edeceğini tutar
    public GameObject beamPrefab;

    public override int scoreEffect { get; set; } = 30;

    public override HashSet<Block> DetermineGroup()
    {
        
        HashSet<Block> blockGroup = new HashSet<Block>();

        foreach (var block in BlockManager.Instance.regularBlocks)
        {
            if (block.blockType == targetColorType)
            {
                blockGroup.Add(block);
                CreateBeams(block.node.transform);
            }
        }

        blockGroup.Add(this);
        return blockGroup;
    }
    private void CreateBeams(Transform targetBlock)
    {
        GameObject beam = Instantiate(beamPrefab, this.transform.position, Quaternion.identity);
        BeamShooter shooter = beam.GetComponent<BeamShooter>();
        shooter.startPoint = this.node.transform;
        shooter.endPoint = targetBlock;
        Destroy(beam,0.5f);
    }
    
}