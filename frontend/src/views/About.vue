<template>
  <div class="about">
    <div class="container">
      <!-- Page Header -->
      <div class="page-header">
        <h1 class="page-title">{{ getSetting('about_title', 'About Us') }}</h1>
        <p class="page-subtitle">
          {{ getSetting('about_subtitle', 'Your trusted partner for powersports adventures') }}
        </p>
      </div>

      <!-- Company Story Section -->
      <section class="section">
        <div class="content-grid">
          <div class="content-text">
            <h2 class="section-title">Our Story</h2>
            <p class="text-content">
              {{ getSetting('about_story_paragraph1', 'Founded with a passion for adventure and the great outdoors, Powersports Gear & Vehicles has been serving the powersports community for over a decade.') }}
            </p>
            <p class="text-content">
              {{ getSetting('about_story_paragraph2', 'Our journey began when our founder, an avid off-road enthusiast, recognized the need for a reliable source of premium powersports equipment.') }}
            </p>
          </div>
          <div class="content-image">
            <img 
              :src="storyImage" 
              alt="Our showroom with various powersports vehicles"
              loading="lazy"
              @error="handleStoryImageError"
            />
          </div>
        </div>
      </section>

      <!-- Mission Section -->
      <section class="section mission-section">
        <div class="content-grid reverse">
          <div class="content-image">
            <img 
              :src="missionImage" 
              alt="Adventure riders exploring trails"
              loading="lazy"
              @error="handleMissionImageError"
            />
          </div>
          <div class="content-text">
            <h2 class="section-title">Our Mission</h2>
            <p class="text-content">
              {{ getSetting('about_mission_text', 'To empower outdoor enthusiasts with the finest powersports vehicles and gear, ensuring every adventure is safe, thrilling, and unforgettable.') }}
            </p>
            <div class="mission-points">
              <div 
                v-for="(point, index) in missionPoints" 
                :key="index" 
                class="mission-point"
              >
                <div class="point-icon">{{ point.icon }}</div>
                <div>
                  <h4>{{ point.title }}</h4>
                  <p>{{ point.description }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Values Section -->
      <section class="section">
        <div class="values-container">
          <h2 class="section-title text-center">Our Values</h2>
          <div class="grid grid-3">
            <div 
              v-for="(value, index) in values" 
              :key="index" 
              class="value-card"
            >
              <div class="value-icon">{{ value.icon }}</div>
              <h3 class="value-title">{{ value.title }}</h3>
              <p class="value-description">
                {{ value.description }}
              </p>
            </div>
          </div>
        </div>
      </section>

      <!-- Team Section -->
      <section class="section team-section">
        <div class="container">
          <h2 class="section-title text-center">Meet Our Team</h2>
          <p class="section-subtitle">
            Passionate experts dedicated to your adventure
          </p>
          
          <div class="grid grid-3">
            <div 
              v-for="(member, index) in teamMembers" 
              :key="index" 
              class="team-member"
            >
              <div class="member-image">
                <img 
                  :src="member.imageUrl" 
                  :alt="member.name"
                  loading="lazy"
                  @error="handleImageError"
                />
              </div>
              <h4 class="member-name">{{ member.name }}</h4>
              <p class="member-role">{{ member.role }}</p>
              <p class="member-bio">
                {{ member.bio }}
              </p>
            </div>
          </div>
        </div>
      </section>

      <!-- Call to Action -->
      <section class="section cta-section">
        <div class="cta-container">
          <h2 class="cta-title">Ready to Start Your Adventure?</h2>
          <p class="cta-text">
            Explore our collection and find the perfect powersports vehicle for your next journey.
          </p>
          <div class="cta-actions">
            <router-link to="/products" class="btn btn-primary btn-lg">
              Shop Products
            </router-link>
            <router-link to="/contact" class="btn btn-outline btn-lg">
              Contact Us
            </router-link>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useSettings } from '@/composables/useSettings';
import { logError } from '@/services/logger';
import { getMediaUrl } from '@/utils/api-config';

const { getSetting } = useSettings();

// Helper function to fix relative URLs and handle empty values
const fixImageUrl = (url: string | null | undefined, defaultUrl: string) => {
  // If empty, null, undefined, or just whitespace, return default
  if (!url || typeof url !== 'string' || url.trim() === '') {
    return defaultUrl;
  }
  // If relative URL, prepend API URL
  if (url.startsWith('/uploads')) {
    return getMediaUrl(url);
  }
  // Return as-is (absolute URL or default)
  return url;
};

// Default images
const DEFAULT_STORY_IMAGE = 'https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=600&h=400&fit=crop';
const DEFAULT_MISSION_IMAGE = 'https://images.unsplash.com/photo-1591737622611-b6c5ae78542d?w=600&h=400&fit=crop';

// Get images with URL fixing
const storyImage = computed(() => {
  const url = getSetting('about_story_image', '');
  return fixImageUrl(url, DEFAULT_STORY_IMAGE);
});

const missionImage = computed(() => {
  const url = getSetting('about_mission_image', '');
  return fixImageUrl(url, DEFAULT_MISSION_IMAGE);
});

// Parse mission points from settings
const missionPoints = computed(() => {
  try {
    const pointsJson = getSetting('about_mission_points', '[]');
    return JSON.parse(pointsJson);
  } catch {
    return [
      { icon: '🎯', title: 'Quality First', description: 'We partner only with trusted manufacturers who share our commitment to excellence.' },
      { icon: '🤝', title: 'Customer Focus', description: 'Your satisfaction drives everything we do, from product selection to after-sales support.' },
      { icon: '🌟', title: 'Innovation', description: 'We stay ahead of industry trends to bring you the latest and greatest in powersports technology.' }
    ];
  }
});

// Parse values from settings
const values = computed(() => {
  try {
    const valuesJson = getSetting('about_values', '[]');
    return JSON.parse(valuesJson);
  } catch {
    return [
      { icon: '🛡️', title: 'Safety', description: 'Safety is paramount in everything we do. We provide only certified, tested equipment and comprehensive safety information.' },
      { icon: '🌍', title: 'Sustainability', description: "We're committed to responsible practices that preserve the natural environments we love to explore." },
      { icon: '🚀', title: 'Performance', description: 'We deliver products that perform when it matters most, built to handle any terrain and weather condition.' }
    ];
  }
});

// Image error handler - fallback to defaults based on alt text
const imageFallbacks: Record<string, string> = {
  'team': 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&h=300&fit=crop&crop=face',
  'showroom': 'https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=600&h=400&fit=crop',
  'adventure': 'https://images.unsplash.com/photo-1591737622611-b6c5ae78542d?w=600&h=400&fit=crop'
};

const handleImageError = (event: Event) => {
  const img = event.target as HTMLImageElement;
  const alt = img.alt.toLowerCase();
  img.src = alt.includes('showroom') ? imageFallbacks.showroom
    : alt.includes('adventure') ? imageFallbacks.adventure
    : imageFallbacks.team;
};

const handleStoryImageError = handleImageError;
const handleMissionImageError = handleImageError;

// Parse team members from settings
const teamMembers = computed(() => {
  const DEFAULT_TEAM_IMAGE = 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&h=300&fit=crop&crop=face';
  
  try {
    const membersJson = getSetting('about_team_members', '[]');
    const members = JSON.parse(membersJson);
    return members.map((member: any) => ({
      ...member,
      imageUrl: fixImageUrl(member.imageUrl, DEFAULT_TEAM_IMAGE)
    }));
  } catch (error) {
    logError('Error parsing team members', error);
    return [
      { name: 'Mike Johnson', role: 'Founder & CEO', bio: '20+ years in powersports with a passion for bringing the best products to fellow enthusiasts.', imageUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&h=300&fit=crop&crop=face' },
      { name: 'Sarah Williams', role: 'Head of Sales', bio: 'Expert in matching customers with their perfect vehicle, with 15 years of industry experience.', imageUrl: 'https://images.unsplash.com/photo-1494790108755-2616b612b786?w=300&h=300&fit=crop&crop=face' },
      { name: 'Tom Rodriguez', role: 'Service Manager', bio: 'Certified technician ensuring every vehicle meets our rigorous quality and safety standards.', imageUrl: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=300&h=300&fit=crop&crop=face' }
    ];
  }
});
</script>

<style scoped>
.about {
  padding: 1rem 0 2rem;
}

.page-header {
  text-align: center;
  margin-bottom: 4rem;
}

.page-title {
  font-size: 2.5rem;
  font-weight: bold;
  color: var(--text-dark);
  margin-bottom: 1rem;
}

.page-subtitle {
  font-size: 1.25rem;
  color: var(--text-light);
}

.section {
  padding: 3rem 0;
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4rem;
  align-items: center;
}

.content-grid.reverse {
  direction: rtl;
}

.content-grid.reverse > * {
  direction: ltr;
}

.content-text {
  flex: 1;
}

.content-image {
  flex: 1;
}

.content-image img {
  width: 100%;
  height: 400px;
  object-fit: cover;
  border-radius: 12px;
  box-shadow: var(--shadow-lg);
}

.section-title {
  font-size: 2rem;
  font-weight: bold;
  color: var(--text-dark);
  margin-bottom: 1.5rem;
}

.text-content {
  font-size: 1.125rem;
  line-height: 1.8;
  color: var(--text-light);
  margin-bottom: 1.5rem;
}

.mission-section {
  background-color: white;
  border-radius: 12px;
  margin: 2rem 0;
  padding: 3rem;
  box-shadow: var(--shadow-md);
}

.mission-points {
  margin-top: 2rem;
}

.mission-point {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.point-icon {
  font-size: 2rem;
  flex-shrink: 0;
}

.mission-point h4 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-dark);
  margin-bottom: 0.5rem;
}

.mission-point p {
  color: var(--text-light);
  line-height: 1.6;
}

.values-container {
  max-width: 1000px;
  margin: 0 auto;
}

.value-card {
  background: white;
  padding: 2.5rem 2rem;
  border-radius: 12px;
  box-shadow: var(--shadow-md);
  text-align: center;
  transition: transform 0.3s ease;
}

.value-card:hover {
  transform: translateY(-4px);
}

.value-icon {
  font-size: 3rem;
  margin-bottom: 1.5rem;
}

.value-title {
  font-size: 1.5rem;
  font-weight: bold;
  color: var(--text-dark);
  margin-bottom: 1rem;
}

.value-description {
  color: var(--text-light);
  line-height: 1.6;
}

.team-section {
  background-color: white;
  border-radius: 12px;
  margin: 2rem 0;
  padding: 3rem;
  box-shadow: var(--shadow-md);
}

.team-member {
  text-align: center;
}

.member-image {
  margin-bottom: 1.5rem;
}

.member-image img {
  width: 150px;
  height: 150px;
  border-radius: 50%;
  object-fit: cover;
  box-shadow: var(--shadow-md);
}

.member-name {
  font-size: 1.25rem;
  font-weight: bold;
  color: var(--text-dark);
  margin-bottom: 0.5rem;
}

.member-role {
  color: var(--primary-color);
  font-weight: 600;
  margin-bottom: 1rem;
  text-transform: uppercase;
  font-size: 0.9rem;
  letter-spacing: 0.5px;
}

.member-bio {
  color: var(--text-light);
  line-height: 1.6;
  font-size: 0.95rem;
}

.cta-section {
  background: linear-gradient(135deg, var(--primary-color), var(--primary-dark));
  color: white;
  border-radius: 12px;
  margin: 2rem 0;
}

.cta-container {
  text-align: center;
  padding: 3rem 2rem;
}

.cta-title {
  font-size: 2.5rem;
  font-weight: bold;
  margin-bottom: 1rem;
}

.cta-text {
  font-size: 1.25rem;
  margin-bottom: 2rem;
  opacity: 0.9;
}

.cta-actions {
  display: flex;
  gap: 1.5rem;
  justify-content: center;
  flex-wrap: wrap;
}

.btn-lg {
  padding: 1rem 2rem;
  font-size: 1.125rem;
}

/* Mobile responsive */
@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }

  .content-grid {
    grid-template-columns: 1fr;
    gap: 2rem;
  }

  .content-image img {
    height: 250px;
  }

  .mission-section {
    padding: 2rem 1.5rem;
    margin: 1rem 0;
  }

  .mission-point {
    align-items: center;
  }

  .team-section {
    padding: 2rem 1rem;
  }

  .value-card {
    padding: 2rem 1.5rem;
  }

  .cta-title {
    font-size: 2rem;
  }

  .cta-actions {
    flex-direction: column;
    align-items: center;
  }

  .btn-lg {
    width: 100%;
    max-width: 300px;
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 1.75rem;
  }

  .section-title {
    font-size: 1.5rem;
  }

  .cta-container {
    padding: 2rem 1rem;
  }

  .cta-title {
    font-size: 1.75rem;
  }

  .section {
    padding: 2rem 0;
  }
}
</style>