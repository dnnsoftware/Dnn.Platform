/*********************************************************************************************************/
/**
 * syntaxhighlight plugin for CKEditor 3.x (Support: Lajox ; Email: lajox@19www.com)
 * Released: On 2009-12-11
 * Download: http://code.google.com/p/lajox
 */
/*********************************************************************************************************/

CKEDITOR.plugins.add('syntaxhighlight',   
  {
      requires: ['dialog'],
      lang: ['de', 'en', 'pl'],
      init: function (a) {
          var b = "syntaxhighlight";
          var c = a.addCommand(b, new CKEDITOR.dialogCommand(b));
          c.modes = { wysiwyg: 1, source: 0 };
          c.canUndo = false;
          a.ui.addButton("syntaxhighlight", {
              label: a.lang.syntaxhighlight.title,
              command: b,
              icon: this.path + "images/syntaxhighlight.gif"
          });
          CKEDITOR.dialog.add(b, this.path + "dialogs/syntaxhighlight.js")
      }
  });