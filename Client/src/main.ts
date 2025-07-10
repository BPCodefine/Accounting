import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import config from 'devextreme/core/config';

config({ licenseKey: 'ewogICJmb3JtYXQiOiAxLAogICJjdXN0b21lcklkIjogIjhmZGNkODRmLTkzY2EtNGZhNS1iMTViLWVmOWZmZmVmZTY5NiIsCiAgIm1heFZlcnNpb25BbGxvd2VkIjogMjQyCn0=.aYKgk5aFarLeTBSbKYcSNQ3EYbzs5HCO0X+RctPadQpwozUliPSwpeMtgZT7ylwdcu368Fp4BWAS3TK4tl/5NOXUIAvTcYffqugslnQT4rrEsUAFpI7HCmCpp3PU6koRaLoFzQ==' });

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
