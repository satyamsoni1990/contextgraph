import { Component } from '@angular/core';
import { marked } from 'marked';
@Component({
  selector: 'app-ai-context',
  imports: [],
  templateUrl: './ai-context.html',
  styleUrl: './ai-context.css',
})
export class AiContext {

  renderMarkdown(text: string): string {
  return marked.parse(text) as string;
}
}
