using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
   /// <summary>
   /// Created by: Alex Barnett
   /// Main controller of cutscene events, uses Coroutines to control timings
   /// </summary>
   
   [SerializeField] private float waitTillDarkness;
   [SerializeField] private float darkTime;
   [SerializeField] private float timeInDarkness;
   [SerializeField] private string dialog1;
   [SerializeField] private string dialog2;
   [SerializeField] private float textDelay;
   [SerializeField] private float delayBetweenDialogs;
   [SerializeField] private float ellipsisDelay;
   [SerializeField] private int ellipsisTimes;
   [SerializeField] private TextMeshProUGUI dialogTextBox;
   [SerializeField] private TextMeshProUGUI dialogTextBox2;
   [SerializeField] private Volume postProcess;

   private Vignette vignette;
   private string ellipsisText;
   private string text;
   
   
   // starts the first coroutine, sets off the cutscene 
   private void Start()
   {
      postProcess.profile.TryGet(out vignette);
      StartCoroutine(WaitTillDark());
   }

   // Skip cutscene with esc key
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
         SceneManager.LoadScene("MainScene");
      }
   }

   private IEnumerator WaitTillDark()
   {
      yield return new WaitForSecondsRealtime(waitTillDarkness);
      StartCoroutine(MakeDark());
   }

   
   // increases vignette intensity to darken the scene, draw focus to main character
   private IEnumerator MakeDark()
   {
      float elapsedTime = 0f;
      while (elapsedTime < darkTime)
      {
         float t = elapsedTime / darkTime;
         vignette.intensity.Override(Mathf.Lerp(0, 1f, t));

         elapsedTime += Time.deltaTime;
         yield return null;
      }
      vignette.intensity.Override(1f);
      yield return new WaitForSecondsRealtime(timeInDarkness);
      StartCoroutine(Dialog1Add());
   }

   // for adding the first dialog one character at a time
   private IEnumerator Dialog1Add()
   {
      char[] dialogChars = dialog1.ToCharArray();
      for (int i = 0; i < dialogChars.Length; i++)
      {
         if (dialogChars[i].Equals(" ".ToCharArray()[0]))
         {
            dialogTextBox.text += "\n ";
         }
         else
         {
            dialogTextBox.text += dialogChars[i];
         }
         yield return new WaitForSecondsRealtime(textDelay);
      }
      StartCoroutine(EllipsisAdd(0,dialogTextBox));
   }

   // for adding the second dialog one character at a time
   private IEnumerator Dialog2Add()
   {
      dialogTextBox.text = "";
      yield return new WaitForSecondsRealtime(delayBetweenDialogs);
      char[] dialogChars = dialog2.ToCharArray();
      for (int i = 0; i < dialogChars.Length; i++)
      {
         if (dialogChars[i].Equals(" ".ToCharArray()[0]))
         {
            dialogTextBox2.text += "\n  ";
         }
         else
         {
            dialogTextBox2.text += dialogChars[i];
         }
         yield return new WaitForSecondsRealtime(textDelay);
      }
      StartCoroutine(EllipsisAdd(1, dialogTextBox2));
   }
   
   // adds the increasing/decreasing ellipsis onto finished dialog text, loads main scene when cutscene over
   private IEnumerator EllipsisAdd(int option, TextMeshProUGUI textBox)
   {
      text = textBox.text;
      yield return new WaitForSecondsRealtime(ellipsisDelay);
      for(int i = 0; i<ellipsisTimes; i++){
         ellipsisText = text + ".";
         textBox.text = ellipsisText;
         yield return new WaitForSecondsRealtime(ellipsisDelay);
         ellipsisText = text + "..";
         textBox.text = ellipsisText;
         yield return new WaitForSecondsRealtime(ellipsisDelay);
         ellipsisText = text + "...";
         textBox.text = ellipsisText;
         yield return new WaitForSecondsRealtime(ellipsisDelay);
      }
      if (option == 0)
      {
         StartCoroutine(Dialog2Add());
      }
      else
      {
         SceneManager.LoadScene("MainScene");
      }
      
   }
   
}
