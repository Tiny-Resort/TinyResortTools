using UnityEngine;

public class moveHitPosToTileHighlighter : MonoBehaviour
{
	public bool up;

	public bool down;

	public bool left;

	public bool right;

	private CharMovement myChar;

	private void OnEnable()
	{
		myChar = base.transform.root.GetComponent<CharMovement>();
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			if (up && left)
			{
				TileHighlighter.highlight.upLeftTransform = base.transform;
			}
			else if (up && right)
			{
				TileHighlighter.highlight.upRightTransform = base.transform;
			}
			else if (up)
			{
				TileHighlighter.highlight.upTransform = base.transform;
			}
			else if (left)
			{
				TileHighlighter.highlight.leftTransform = base.transform;
			}
			else if (right)
			{
				TileHighlighter.highlight.rightTransform = base.transform;
			}
		}
	}

	private void OnDestroy()
	{
		clearTransforms();
	}

	public void clearTransforms()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			if (up && left && TileHighlighter.highlight.upLeftTransform == base.transform)
			{
				TileHighlighter.highlight.upLeftTransform = null;
			}
			else if (up && right && TileHighlighter.highlight.upRightTransform == base.transform)
			{
				TileHighlighter.highlight.upRightTransform = null;
			}
			else if (up && TileHighlighter.highlight.upTransform == base.transform)
			{
				TileHighlighter.highlight.upTransform = null;
			}
			else if (left && TileHighlighter.highlight.leftTransform == base.transform)
			{
				TileHighlighter.highlight.leftTransform = null;
			}
			else if (right && TileHighlighter.highlight.rightTransform == base.transform)
			{
				TileHighlighter.highlight.rightTransform = null;
			}
			TileHighlighter.highlight.updateSides();
		}
	}

	private void Update()
	{
		base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, Mathf.Round(base.transform.root.eulerAngles.y / 90f) * 90f, base.transform.eulerAngles.z);
		if (up && left)
		{
			base.transform.position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position + base.transform.forward * 2f + -base.transform.right * 2f;
		}
		else if (up && right)
		{
			base.transform.position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position + base.transform.forward * 2f + base.transform.right * 2f;
		}
		else if (up)
		{
			base.transform.position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position + base.transform.forward * 2f;
		}
		else if (down)
		{
			base.transform.position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position + -base.transform.forward * 2f;
		}
		else if (left)
		{
			base.transform.position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position + -base.transform.right * 2f;
		}
		else if (right)
		{
			base.transform.position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position + base.transform.right * 2f;
		}
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			TileHighlighter.highlight.updateSides();
		}
	}
}
