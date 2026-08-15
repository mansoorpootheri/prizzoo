import styles from "./SplashScreen.module.css";

export function SplashScreen() {
  return (
    <div className={styles.root} data-testid="splash-screen">
      <img
        src="/assets/splash/radial-glow.png"
        alt=""
        aria-hidden="true"
        className={styles.glowTop}
      />
      <img
        src="/assets/splash/radial-glow.png"
        alt=""
        aria-hidden="true"
        className={styles.glowBottom}
      />
      <div className={styles.content}>
        <img
          src="/assets/splash/prizzoo-logo.png"
          alt="PriZzoO.com"
          className={styles.logo}
        />
        <p className={styles.tagline}>Compare. Locate. Save</p>
      </div>
    </div>
  );
}
