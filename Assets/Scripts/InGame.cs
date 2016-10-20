using UnityEngine;
using System.Collections;
using AC;

public class InGame : MonoBehaviour {
    int ciclo = 0;
    [Header("Inventory")]
    UITexture inventorySelectedItemTexture;
    UILabel inventoryTitle;
    UILabel inventoryDescription;
    UILabel inventoryInstructions;
    public TweenAlpha panelInventoryTween;
    bool inventoryShowing = false;
    //para mostrar itemes automaticamente cuando se recojen
    bool inventoryAutoShow = false;

    [Header("Instructions")]
    UILabel instructionsLabel;
    public TweenAlpha instructionsTween;
    public string[] instructions;
	// Use this for initialization
	void Start () {
        inventorySelectedItemTexture = panelInventoryTween.transform.FindChild("InventoryItemSelected").GetComponent<UITexture>();
        inventoryTitle = panelInventoryTween.transform.FindChild("Title").GetComponent<UILabel>();
        inventoryDescription = panelInventoryTween.transform.FindChild("Description").GetComponent<UILabel>();
        inventoryInstructions = panelInventoryTween.transform.FindChild("Instructions").GetComponent<UILabel>();

        instructionsLabel = instructionsTween.transform.FindChild("Instructions").GetComponent<UILabel>();

        inventorySelectedItemTexture.mainTexture = null;
    }

    public void showInstructions(int index)
    {
        string text = instructions[index];
        /*
        switch (index)
        {
            case 0:
                text = "Press [Mouse right button] to bring cellphone up";
                break;
            case 1:
                text = "Select the lightbulb icon and press [Mouse left button]";
                break;
            case 2:
                text = "Press [Mouse right button] to hide the cellphone";
                break;
        }*/
        instructionsLabel.text = text;
        instructionsTween.PlayForward();
    }

    public void hideInstructions()
    {
        instructionsTween.PlayReverse();
    }
	
	// Update is called once per frame
	void Update () {
        AC.InvItem selectedItem = AC.KickStarter.runtimeInventory.selectedItem;
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryShowing = !inventoryShowing;
            if (!inventoryShowing)
            {
                inventoryShowing = false;
                clearInventoryItem();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryShowing)
            {
                inventoryShowing = false;
                clearInventoryItem();
            }
        }
        //print(AC.KickStarter.runtimeInventory.);

        if (selectedItem != null)
        {
            string title = selectedItem.GetProperty(0).GetValue();
            string description = selectedItem.GetProperty(1).GetValue();
            description = description.Replace("\\n", "\n");
            string instructions = selectedItem.GetProperty(2).GetValue();
            instructions = instructions.Replace("\\n", "\n");
            string spriteName = selectedItem.GetProperty(3).GetValue();
            // Assign the description to the UI text-box
            ciclo++;
            if(Input.GetMouseButtonUp(0) || inventoryAutoShow)
            {
                AC.KickStarter.runtimeInventory.selectedItem = null;
                inventoryAutoShow = false;
                //Se interpreta como click
                if (ciclo < 10)
                {
                    //AC.KickStarter.menuManager.GetSelectedMenu().GetElementWithName("ItemDetail").isVisible = true;
                    //AC.MenuGraphic m = AC.KickStarter.menuManager.GetSelectedMenu().GetElementWithName("ItemDetail") as AC.MenuGraphic;
                    if (inventorySelectedItemTexture.mainTexture != null && inventorySelectedItemTexture.mainTexture.name == spriteName)
                    {
                        clearInventoryItem();
                    }
                    else
                    {
                        panelInventoryTween.PlayForward();
                        inventorySelectedItemTexture.mainTexture = Resources.Load<Texture2D>("Items/" + spriteName);
                        this.inventoryTitle.text = title;
                        this.inventoryDescription.text = description;
                        this.inventoryInstructions.text = instructions;
                    }
                    //selectItemInteraction.Interact();
                    //m.graphic.texture = m.graphic.GetAnimatedSprite(2).texture;
                    //m.graphic.Draw(Vector2.zero);
                    //AdvGame.PlayAnimClipFrame(AC.PlayerMenus.GetMenuWithName("Inventory").canvas.gameObject);
                    //m. = selectedItem.activeTex;
                    //print(AC.KickStarter.menuManager.menus[5].elements[3].backgroundTexture.name);
                }
                ciclo = 0;
            }
        }
    }

    public void clearInventoryItem()
    {
        panelInventoryTween.PlayReverse();
    }

    //llamado desde el termino de animacion del panelInventoryTween
    public void resetVariablesInventory()
    {
        if (panelInventoryTween.direction == AnimationOrTween.Direction.Reverse)
        {
            inventorySelectedItemTexture.mainTexture = null;
            this.inventoryTitle.text = "";
            this.inventoryDescription.text = "";
            this.inventoryInstructions.text = "";
        }
    }

    public void inventoryAutoshowActivate()
    {
        inventoryShowing = true;
        inventoryAutoShow = true;
    }
}
