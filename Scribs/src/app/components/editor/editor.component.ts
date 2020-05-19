import { Component, OnInit, Input } from '@angular/core';
import { Storage } from '@ionic/storage';

import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';

@Component({
  selector: 's-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.scss'],
})
export class EditorComponent implements OnInit {
  @Input() context: WorkspaceContext;
  theme: string;
  buttons: string[];
  options: Object;
  
  constructor(private storage: Storage) { }

  ngOnInit() {
    this.storage.get('theme').then((val) => {
      this.theme = val;
      if (this.theme === undefined || this.theme === null || this.theme === 'light') {
        this.theme = 'gray';
      }
      this.buttons = [
        'bold',
        'italic',
        'underline',
        'paragraphFormat',
        'specialCharacters',
        'align',
        'print',
        // 'fullscreen',
        'undo',
        'redo',
        'alert',
        'save'
      ];
      this.options = {
        charCounterCount: true,
        theme: this.theme,
        tooltips: false,
        // height:'750',
        inlineMode: false,
        // heightMax: window.innerHeight - 56,
        // width: 600,
        toolbarSticky: true,
        attribution: false,
        // scrollableContainer: '#scrollable',
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
          // 'froalaEditor.save.before' : (e: any, editor: any) => {
            // e.stopPropagation();
            // this.save();
          // },
          // 'froalaEditor.contentChanged' : (e: any, editor: any) => {
          //   if (this.file) {
          //     this.file.changed = true;
          //   }
          // }
        },
        // toolbarStickyOffset: 48
      };
    });}

}
