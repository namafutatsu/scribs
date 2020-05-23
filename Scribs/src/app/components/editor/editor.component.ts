import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Storage } from '@ionic/storage';

import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';

@Component({
  selector: 's-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.scss'],
})
export class EditorComponent implements OnInit {
  @Input() context: WorkspaceContext;
  @Output() saving = new EventEmitter();
  theme: string;
  buttons = [
    'bold',
    'italic',
    'underline',
    'paragraphFormat',
    'specialCharacters',
    'align',
    'print',
    'fullscreen',
    'undo',
    'redo',
    'alert',
    'save'
  ];
  options: Object;
  
  constructor(private storage: Storage) { }

  ngOnInit() {
    this.storage.get('theme').then((val) => {
      this.theme = val;
      if (this.theme === undefined || this.theme === null || this.theme === 'light') {
        this.theme = 'gray';
      }
      this.options = {
        charCounterCount: true,
        theme: this.theme,
        tooltips: false,
        inlineMode: false,
        // scrollableContainer: '#editor-wrapper',
        toolbarSticky: true,
        toolbarStickyOffset: 56,
        attribution: false,
        pluginsEnabled: [
          'align',
          // 'charCounter',
          // 'codeBeautifier',
          // 'codeView',
          // 'colors',
          'draggable',
          // 'emoticons',
          'entities',
          // 'file',
          // 'fontFamily',
          'fontSize',
          // 'fullscreen',
          // 'image',
          // 'imageManager',
          'inlineStyle',
          'lineBreaker',
          // 'link',
          'lists',
          'paragraphFormat',
          'paragraphStyle',
          // 'quickInsert',
          // 'quote',
          'save',
          // 'table',
          // 'url',
          // 'video',
          'wordPaste',
          'specialCharacters',
          'wordPaste',
          'print'
        ],
        // shortcutsEnabled: [
        //   'show',
        //   'bold',
        //   'italic',
        //   'underline',
        //   // 'strikeThrough',
        //   'indent',
        //   'outdent',
        //   'undo',
        //   'redo',
        //   // 'insertImage',
        //   'createLink','paragraphFormat'
        // ],
        toolbarButtons: this.buttons,
        toolbarButtonsSM: this.buttons,
        toolbarButtonsMD: this.buttons,
        toolbarButtonsXS: this.buttons,
        events : {
          'save.before' : () => {
            this.save();
          },
          'contentChanged' : () => {
            this.context.syncTexts[this.context.open] = true;
          }
        },
        // toolbarStickyOffset: 48
      };
    });
    // $.FroalaEditor.RegisterShortcut(83, 'save');
    // $.FroalaEditor.DefineIcon("customSave", {
    //   NAME: "floppy-o",
    //   template: "font_awesome"
    // });
  }

  save() {
    this.saving.emit();
  }
}
