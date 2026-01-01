---
layout: default
title: Home
nav_order: 1
---

<div class="hero">
  <h1>Technical Babbling</h1>
  <p>Exploring the depths of Java, C#, Performance, and Software Engineering.</p>
</div>

<div class="posts-grid">
  {% for post in site.posts %}
    <div class="post-card">
      <div class="post-content">
        <h3><a class="post-link" href="{{ post.url | absolute_url }}">{{ post.title }}</a></h3>
        <div class="post-excerpt">
          {{ post.excerpt | strip_html | truncatewords: 30 }}
        </div>
      </div>
      <div class="post-meta">
        <span class="post-date">{{ post.date | date: "%b %d, %Y" }}</span>
        <span class="read-time">Read More &rarr;</span>
      </div>
      <a href="{{ post.url | absolute_url }}" class="post-link" style="position:absolute; top:0; left:0; width:100%; height:100%; z-index:1;"></a>
    </div>
  {% endfor %}
</div>