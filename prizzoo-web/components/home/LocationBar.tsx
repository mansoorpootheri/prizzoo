import styles from "./LocationBar.module.css";

interface LocationBarProps {
  isLocating: boolean;
  isFallback: boolean;
}

export function LocationBar({ isLocating, isFallback }: LocationBarProps) {
  return (
    <div className={styles.root}>
      <div className={styles.left}>
        <img
          src="/assets/home/location-icon.svg"
          alt=""
          aria-hidden="true"
          className={styles.pin}
        />
        <div className={styles.text}>
          <span className={styles.city}>
            {isLocating ? "Locating…" : isFallback ? "Location unavailable" : "Current location"}
          </span>
          <span className={styles.country}>
            {isFallback ? "Showing default results" : "Nearby stores"}
          </span>
        </div>
      </div>
      <img
        src="/assets/home/camera-icon.svg"
        alt=""
        aria-hidden="true"
        className={styles.cameraIcon}
      />
    </div>
  );
}
