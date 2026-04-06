import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SeoService } from '../../../core/services/seo.service';
import { FaIconComponent, IconDefinition } from '@fortawesome/angular-fontawesome';
import { faFileLines } from '@fortawesome/free-solid-svg-icons';
import { AUTHOR_CONFIG, AuthorConfig } from '../../../core/models/author.config';

@Component({
  selector: 'app-about-me',
  imports: [CommonModule, RouterModule, FaIconComponent],
  templateUrl: './about-me.html',
  styleUrl: './about-me.css',
})
export class AboutMe implements OnInit {
  private seo = inject(SeoService);

  readonly author: AuthorConfig = AUTHOR_CONFIG;
  ngOnInit(): void {
    this.seo.setPage({
      title: `${this.author.firstName} ${this.author.lastName}`,
      description: `${this.author.role} - Cyberius`,
    });
  }

  protected readonly faFileLines = faFileLines;
}
