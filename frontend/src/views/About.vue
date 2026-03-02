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
              Founded with a passion for adventure and the great outdoors, Powersports Gear & Vehicles 
              has been serving the powersports community for over a decade. We specialize in high-quality 
              ATVs, dirt bikes, UTVs, snowmobiles, and gear for adventure seekers who demand the best.
            </p>
            <p class="text-content">
              Our journey began when our founder, an avid off-road enthusiast, recognized the need for 
              a reliable source of premium powersports equipment. Today, we've grown into a trusted name 
              in the industry, serving customers across the country with top-tier products and exceptional service.
            </p>
          </div>
          <div class="content-image">
            <img 
              src="https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=600&h=400&fit=crop" 
              alt="Our showroom with various powersports vehicles"
              loading="lazy"
            />
          </div>
        </div>
      </section>

      <!-- Mission Section -->
      <section class="section mission-section">
        <div class="content-grid reverse">
          <div class="content-image">
            <img 
              src="https://images.unsplash.com/photo-1591737622611-b6c5ae78542d?w=600&h=400&fit=crop" 
              alt="Adventure riders exploring trails"
              loading="lazy"
            />
          </div>
          <div class="content-text">
            <h2 class="section-title">Our Mission</h2>
            <p class="text-content">
              To empower outdoor enthusiasts with the finest powersports vehicles and gear, 
              ensuring every adventure is safe, thrilling, and unforgettable. We believe that 
              the right equipment doesn't just enhance your experience—it transforms it.
            </p>
            <div class="mission-points">
              <div class="mission-point">
                <div class="point-icon">🎯</div>
                <div>
                  <h4>Quality First</h4>
                  <p>We partner only with trusted manufacturers who share our commitment to excellence.</p>
                </div>
              </div>
              <div class="mission-point">
                <div class="point-icon">🤝</div>
                <div>
                  <h4>Customer Focus</h4>
                  <p>Your satisfaction drives everything we do, from product selection to after-sales support.</p>
                </div>
              </div>
              <div class="mission-point">
                <div class="point-icon">🌟</div>
                <div>
                  <h4>Innovation</h4>
                  <p>We stay ahead of industry trends to bring you the latest and greatest in powersports technology.</p>
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
            <div class="value-card">
              <div class="value-icon">🛡️</div>
              <h3 class="value-title">Safety</h3>
              <p class="value-description">
                Safety is paramount in everything we do. We provide only certified, 
                tested equipment and comprehensive safety information.
              </p>
            </div>
            <div class="value-card">
              <div class="value-icon">🌍</div>
              <h3 class="value-title">Sustainability</h3>
              <p class="value-description">
                We're committed to responsible practices that preserve the 
                natural environments we love to explore.
              </p>
            </div>
            <div class="value-card">
              <div class="value-icon">🚀</div>
              <h3 class="value-title">Performance</h3>
              <p class="value-description">
                We deliver products that perform when it matters most, 
                built to handle any terrain and weather condition.
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
            <div class="team-member">
              <div class="member-image">
                <img 
                  src="https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&h=300&fit=crop&crop=face" 
                  alt="Team member"
                  loading="lazy"
                />
              </div>
              <h4 class="member-name">Mike Johnson</h4>
              <p class="member-role">Founder & CEO</p>
              <p class="member-bio">
                20+ years in powersports with a passion for bringing 
                the best products to fellow enthusiasts.
              </p>
            </div>
            
            <div class="team-member">
              <div class="member-image">
                <img 
                  src="https://images.unsplash.com/photo-1494790108755-2616b612b786?w=300&h=300&fit=crop&crop=face" 
                  alt="Team member"
                  loading="lazy"
                />
              </div>
              <h4 class="member-name">Sarah Williams</h4>
              <p class="member-role">Head of Sales</p>
              <p class="member-bio">
                Expert in matching customers with the perfect 
                vehicle for their adventure needs and budget.
              </p>
            </div>
            
            <div class="team-member">
              <div class="member-image">
                <img 
                  src="https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=300&h=300&fit=crop&crop=face" 
                  alt="Team member"
                  loading="lazy"
                />
              </div>
              <h4 class="member-name">Tom Rodriguez</h4>
              <p class="member-role">Technical Specialist</p>
              <p class="member-bio">
                Certified technician ensuring every vehicle meets 
                our high standards before reaching customers.
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
import { useSettings } from '@/composables/useSettings';

const { getSetting } = useSettings();
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