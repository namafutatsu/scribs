import { Entity } from './entity';

export class Document extends Entity {
  tempId: string;
  children: Document[];
  index: number;
  isLeaf: boolean;
  isDirectory: boolean;
}

export class Project extends Document {

}

export class Text extends Entity {
  content: string;
}

export class Workspace {
  project: Project;
  texts: {};
}