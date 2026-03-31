import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Hero } from './components/hero/hero';
import { FeaturedPosts } from './components/featured-posts/featured-posts';
import { Newsletter } from './components/newsletter/newsletter';
import { TechStack } from './components/tech-stack/tech-stack';
import { TopPost } from './components/top-post/top-post';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    Hero,
    TopPost,
    FeaturedPosts,
    Newsletter,
    TechStack
  ],
  templateUrl: './home.component.html',
})
export class HomeComponent {}
