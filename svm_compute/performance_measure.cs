using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using svm_compute;

namespace svm_compute
{
    

    public static class performance_measure
    {
        public class prediction
        {
            public int prediction_index;
            public int actual_class;
            public int default_predicted_class;
            public bool probability_estimates_stated;
            public List<(int class_id, double probability_estimate)> probability_estimates;
            public string[] test_row_vector;
            public string comment;
        }

        public class confusion_matrix
        {









































































            public List<double> thresholds;
            public string roc_xy_str_all;
            public string roc_xy_str_11p;
            public string pr_xy_str_all;
            public string pr_xy_str_11p;
            public string pri_xy_str_all;
            public string pri_xy_str_11p;
            public string duration_grid_search;
            public string duration_nm_search;
            public string duration_training;
            public string duration_testing;
            public string testing_set_name;
            //public string experiment_id1;
            //public string experiment_id2;
            //public string experiment_id3;
            
            //public string fs_starting_point;
            //public string feature_selection_type;
            //public string fs_algorithm_direction;
            //public string fs_perf_selection;
            //public string math_operation;
            public string scaling_method;

            public double libsvm_cv;
            public double libsvm_cv_precision;
            public double libsvm_cv_recall;
            public double libsvm_cv_fscore;
            public double libsvm_cv_bac;
            public double libsvm_cv_auc;
            public double libsvm_cv_accuracy;
            public double libsvm_cv_ap;
            
            public double? prediction_threshold = -1;
            public double? prediction_threshold_class;
            public int feature_count;
            public int group_count;
            public string features_included;
            public string training_resampling_method;
            public double training_size_pct;
            public double unused_size_pct;
            public double testing_size_pct;
            public string kernel_parameter_search_method;
            public double inner_cv_folds;
            public double randomisation_cv_index;
            public double randomisation_cv_folds;
            public double outer_cv_index;
            public double outer_cv_folds;
            public string svm_type;
            public string kernel;
            public double? cost;
            public double? gamma;
            public double? epsilon;
            public double? coef0;
            public double? degree;
            
            public int? class_id;
            public double? class_weight;
            public string class_name;
            public double class_size;
            public double class_training_size;
            public double P;
            public double N;
            public double TP;
            public double FP;
            public double TN;
            public double FN;
            public double TPR;
            public double TNR;
            public double PPV;
            public double Precision;
            public double Prevalence;
            public double MCR;
            public double ER;
            public double NER;
            public double CNER;
            public double Kappa;
            public double Overlap;
            public double RND_ACC;
            public double Support;
            public double BaseRate;
            public double Youden;
            public double NPV;
            public double FNR;
            public double FPR;
            public double FDR;
            public double FOR;
            public double ACC;
            public double GM;
            public double F1S;
            public double G1S;
            public double MCC;
            public double BM_;
            public double MK_;
            public double BAC;
            public double ROC_AUC_Approx_All;
            public double ROC_AUC_Approx_11p;
            public double ROC_AUC_All;
            //public double ROC_AUC_11p;
            public double PR_AUC_Approx_All;
            public double PR_AUC_Approx_11p;
            public double PRI_AUC_Approx_All;
            public double PRI_AUC_Approx_11p;
            public double AP_All;
            public double AP_11p;
            public double API_All;
            public double API_11p;
            public double Brier_All;
            public double LRP;
            public double LRN;
            public double F1B_00;
            public double F1B_01;
            public double F1B_02;
            public double F1B_03;
            public double F1B_04;
            public double F1B_05;
            public double F1B_06;
            public double F1B_07;
            public double F1B_08;
            public double F1B_09;
            public double F1B_10;

            public double PPF_TP;
            public double PPF_FP;
            public double PPF_TN;
            public double PPF_FN;
            public double PPF_TPR;
            public double PPF_TNR;
            public double PPF_PPV;
            public double PPF_Precision;
            public double PPF_Prevalence;
            public double PPF_MCR;
            public double PPF_ER;
            public double PPF_NER;
            public double PPF_CNER;
            public double PPF_Kappa;
            public double PPF_Overlap;
            public double PPF_RND_ACC;
            public double PPF_Support;
            public double PPF_BaseRate;
            public double PPF_Youden;
            public double PPF_NPV;
            public double PPF_FNR;
            public double PPF_FPR;
            public double PPF_FDR;
            public double PPF_FOR;
            public double PPF_ACC;
            public double PPF_GM;
            public double PPF_F1S;
            public double PPF_G1S;
            public double PPF_MCC;
            public double PPF_BM_;
            public double PPF_MK_;
            public double PPF_BAC;
            public double PPF_ROC_AUC_Approx_All;
            public double PPF_ROC_AUC_Approx_11p;
            public double PPF_ROC_AUC_All;
            //public double PPF_ROC_AUC_11p;
            public double PPF_PR_AUC_Approx_All;
            public double PPF_PR_AUC_Approx_11p;
            public double PPF_PRI_AUC_Approx_All;
            public double PPF_PRI_AUC_Approx_11p;
            public double PPF_AP_All;
            public double PPF_AP_11p;
            public double PPF_API_All;
            public double PPF_API_11p;
            public double PPF_Brier_All;
            public double PPF_LRP;
            public double PPF_LRN;
            public double PPF_F1B_00;
            public double PPF_F1B_01;
            public double PPF_F1B_02;
            public double PPF_F1B_03;
            public double PPF_F1B_04;
            public double PPF_F1B_05;
            public double PPF_F1B_06;
            public double PPF_F1B_07;
            public double PPF_F1B_08;
            public double PPF_F1B_09;
            public double PPF_F1B_10;

            public double PPG_TP;
            public double PPG_FP;
            public double PPG_TN;
            public double PPG_FN;
            public double PPG_TPR;
            public double PPG_TNR;
            public double PPG_PPV;
            public double PPG_Precision;
            public double PPG_Prevalence;
            public double PPG_MCR;
            public double PPG_ER;
            public double PPG_NER;
            public double PPG_CNER;
            public double PPG_Kappa;
            public double PPG_Overlap;
            public double PPG_RND_ACC;
            public double PPG_Support;
            public double PPG_BaseRate;
            public double PPG_Youden;
            public double PPG_NPV;
            public double PPG_FNR;
            public double PPG_FPR;
            public double PPG_FDR;
            public double PPG_FOR;
            public double PPG_ACC;
            public double PPG_GM;
            public double PPG_F1S;
            public double PPG_G1S;
            public double PPG_MCC;
            public double PPG_BM_;
            public double PPG_MK_;
            public double PPG_BAC;
            public double PPG_ROC_AUC_Approx_All;
            public double PPG_ROC_AUC_Approx_11p;
            public double PPG_ROC_AUC_All;
            //public double PPG_ROC_AUC_11p;
            public double PPG_PR_AUC_Approx_All;
            public double PPG_PR_AUC_Approx_11p;
            public double PPG_PRI_AUC_Approx_All;
            public double PPG_PRI_AUC_Approx_11p;
            public double PPG_AP_All;
            public double PPG_AP_11p;
            public double PPG_API_All;
            public double PPG_API_11p;
            public double PPG_Brier_All;
            public double PPG_LRP;
            public double PPG_LRN;
            public double PPG_F1B_00;
            public double PPG_F1B_01;
            public double PPG_F1B_02;
            public double PPG_F1B_03;
            public double PPG_F1B_04;
            public double PPG_F1B_05;
            public double PPG_F1B_06;
            public double PPG_F1B_07;
            public double PPG_F1B_08;
            public double PPG_F1B_09;
            public double PPG_F1B_10;

            public List<double> get_values(cross_validation_metrics cross_validation_metrics)
            {
                var metric_values = new List<double>();

                if (cross_validation_metrics == 0) throw new Exception();

                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TP)) { metric_values.Add(this.TP); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FP)) { metric_values.Add(this.FP); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TN)) { metric_values.Add(this.TN); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FN)) { metric_values.Add(this.FN); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TPR)) { metric_values.Add(this.TPR); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TNR)) { metric_values.Add(this.TNR); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PPV)) { metric_values.Add(this.PPV); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Precision)) { metric_values.Add(this.Precision); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Prevalence)) { metric_values.Add(this.Prevalence); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.MCR)) { metric_values.Add(this.MCR); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ER)) { metric_values.Add(this.ER); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.NER)) { metric_values.Add(this.NER); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.CNER)) { metric_values.Add(this.CNER); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Kappa)) { metric_values.Add(this.Kappa); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Overlap)) { metric_values.Add(this.Overlap); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.RND_ACC)) { metric_values.Add(this.RND_ACC); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Support)) { metric_values.Add(this.Support); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.BaseRate)) { metric_values.Add(this.BaseRate); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Youden)) { metric_values.Add(this.Youden); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.NPV)) { metric_values.Add(this.NPV); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FNR)) { metric_values.Add(this.FNR); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FPR)) { metric_values.Add(this.FPR); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FDR)) { metric_values.Add(this.FDR); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FOR)) { metric_values.Add(this.FOR); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ACC)) { metric_values.Add(this.ACC); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.GM)) { metric_values.Add(this.GM); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1S)) { metric_values.Add(this.F1S); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.G1S)) { metric_values.Add(this.G1S); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.MCC)) { metric_values.Add(this.MCC); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.BM_)) { metric_values.Add(this.BM_); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.MK_)) { metric_values.Add(this.MK_); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.BAC)) { metric_values.Add(this.BAC); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_Approx_All)) { metric_values.Add(this.ROC_AUC_Approx_All); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_Approx_11p)) { metric_values.Add(this.ROC_AUC_Approx_11p); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_All)) { metric_values.Add(this.ROC_AUC_All); }
                //if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC2_11p)) { metric_values.Add(this.ROC_AUC_11p); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PR_AUC_Approx_All)) { metric_values.Add(this.PR_AUC_Approx_All); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PR_AUC_Approx_11p)) { metric_values.Add(this.PR_AUC_Approx_11p); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PRI_AUC_Approx_All)) { metric_values.Add(this.PRI_AUC_Approx_All); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PRI_AUC_Approx_11p)) { metric_values.Add(this.PRI_AUC_Approx_11p); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.AP_All)) { metric_values.Add(this.AP_All); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.AP_11p)) { metric_values.Add(this.AP_11p); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.API_All)) { metric_values.Add(this.API_All); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.API_11p)) { metric_values.Add(this.API_11p); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Brier_All)) { metric_values.Add(this.Brier_All); }
                //if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Brier_11p)) { metric_values.Add(this.Brier_11p); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.LRP)) { metric_values.Add(this.LRP); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.LRN)) { metric_values.Add(this.LRN); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_00)) { metric_values.Add(this.F1B_00); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_01)) { metric_values.Add(this.F1B_01); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_02)) { metric_values.Add(this.F1B_02); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_03)) { metric_values.Add(this.F1B_03); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_04)) { metric_values.Add(this.F1B_04); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_05)) { metric_values.Add(this.F1B_05); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_06)) { metric_values.Add(this.F1B_06); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_07)) { metric_values.Add(this.F1B_07); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_08)) { metric_values.Add(this.F1B_08); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_09)) { metric_values.Add(this.F1B_09); }
                if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_10)) { metric_values.Add(this.F1B_10); }

                return metric_values;
            }

            public (string name, double value)[] get_value_strings()
            {
                var result = new (string name, double value)[] {

                    (nameof(TP), TP),
                    (nameof(FP), FP),
                    (nameof(TN), TN),
                    (nameof(FN), FN),
                    (nameof(TPR), TPR),
                    (nameof(TNR), TNR),
                    (nameof(PPV), PPV),
                    (nameof(Precision), Precision),
                    (nameof(Prevalence), Prevalence),
                    (nameof(MCR), MCR),
                    (nameof(ER), ER),
                    (nameof(NER), NER),
                    (nameof(CNER), CNER),
                    (nameof(Kappa), Kappa),
                    (nameof(Overlap), Overlap),
                    (nameof(RND_ACC), RND_ACC),
                    (nameof(Support), Support),
                    (nameof(BaseRate), BaseRate),
                    (nameof(Youden), Youden),
                    (nameof(NPV), NPV),
                    (nameof(FNR), FNR),
                    (nameof(FPR), FPR),
                    (nameof(FDR), FDR),
                    (nameof(FOR), FOR),
                    (nameof(ACC), ACC),
                    (nameof(GM), GM),
                    (nameof(F1S), F1S),
                    (nameof(G1S), G1S),
                    (nameof(MCC), MCC),
                    (nameof(BM_), BM_),
                    (nameof(MK_), MK_),
                    (nameof(BAC), BAC),
                    (nameof(ROC_AUC_Approx_All), ROC_AUC_Approx_All),
                    (nameof(ROC_AUC_Approx_11p), ROC_AUC_Approx_11p),
                    (nameof(ROC_AUC_All), ROC_AUC_All),
                    //(nameof(ROC_AUC_11p), ROC_AUC_11p),
                    (nameof(PR_AUC_Approx_All), PR_AUC_Approx_All),
                    (nameof(PR_AUC_Approx_11p), PR_AUC_Approx_11p),
                    (nameof(PRI_AUC_Approx_All), PRI_AUC_Approx_All),
                    (nameof(PRI_AUC_Approx_11p), PRI_AUC_Approx_11p),
                    (nameof(AP_All), AP_All),
                    (nameof(AP_11p), AP_11p),
                    (nameof(API_All), API_All),
                    (nameof(API_11p), API_11p),
                    (nameof(Brier_All), Brier_All),
                    (nameof(LRP), LRP),
                    (nameof(LRN), LRN),
                    (nameof(F1B_00), F1B_00),
                    (nameof(F1B_01), F1B_01),
                    (nameof(F1B_02), F1B_02),
                    (nameof(F1B_03), F1B_03),
                    (nameof(F1B_04), F1B_04),
                    (nameof(F1B_05), F1B_05),
                    (nameof(F1B_06), F1B_06),
                    (nameof(F1B_07), F1B_07),
                    (nameof(F1B_08), F1B_08),
                    (nameof(F1B_09), F1B_09),
                    (nameof(F1B_10), F1B_10),
                };

                return result;
            }

            [Flags]
            public enum cross_validation_metrics : ulong
            {
                None = 0UL,
                TP = 1UL << 01,
                FP = 1UL << 02,
                TN = 1UL << 03,
                FN = 1UL << 04,
                TPR = 1UL << 05,
                TNR = 1UL << 06,
                PPV = 1UL << 07,
                Precision = 1UL << 08,
                Prevalence = 1UL << 09,
                MCR = 1UL << 10,
                ER = 1UL << 11,
                NER = 1UL << 12,
                CNER = 1UL << 13,
                Kappa = 1UL << 14,
                Overlap = 1UL << 15,
                RND_ACC = 1UL << 16,
                Support = 1UL << 17,
                BaseRate = 1UL << 18,
                Youden = 1UL << 19,
                NPV = 1UL << 20,
                FNR = 1UL << 21,
                FPR = 1UL << 22,
                FDR = 1UL << 23,
                FOR = 1UL << 24,
                ACC = 1UL << 25,
                GM = 1UL << 26,
                F1S = 1UL << 27,
                G1S = 1UL << 28,
                MCC = 1UL << 29,
                BM_ = 1UL << 30,
                MK_ = 1UL << 31,
                BAC = 1UL << 32,
                ROC_AUC_Approx_All = 1UL << 33,
                ROC_AUC_Approx_11p = 1UL << 34,
                ROC_AUC_All = 1UL << 35,
                //ROC_AUC_11p = 1UL << 36,
                PR_AUC_Approx_All = 1UL << 36,
                PR_AUC_Approx_11p = 1UL << 37,
                PRI_AUC_Approx_All = 1UL << 38,
                PRI_AUC_Approx_11p = 1UL << 39,
                AP_All = 1UL << 40,
                AP_11p = 1UL << 41,
                API_All = 1UL << 42,
                API_11p = 1UL << 43,
                Brier_All = 1UL << 44,
                LRP = 1UL << 45,
                LRN = 1UL << 46,
                F1B_00 = 1UL << 47,
                F1B_01 = 1UL << 48,
                F1B_02 = 1UL << 49,
                F1B_03 = 1UL << 50,
                F1B_04 = 1UL << 51,
                F1B_05 = 1UL << 52,
                F1B_06 = 1UL << 53,
                F1B_07 = 1UL << 54,
                F1B_08 = 1UL << 55,
                F1B_09 = 1UL << 56,
                F1B_10 = 1UL << 57,
            }


            public void calculate_ppf()
            {
                if (feature_count != 0)
                {
                    PPF_TP = TP / (double)feature_count;
                    PPF_FP = FP / (double)feature_count;
                    PPF_TN = TN / (double)feature_count;
                    PPF_FN = FN / (double)feature_count;
                    PPF_TPR = TPR / (double)feature_count;
                    PPF_TNR = TNR / (double)feature_count;
                    PPF_PPV = PPV / (double)feature_count;
                    PPF_Precision = Precision / (double)feature_count;
                    PPF_Prevalence = Prevalence / (double)feature_count;
                    PPF_MCR = MCR / (double)feature_count;
                    PPF_ER = ER / (double)feature_count;
                    PPF_NER = NER / (double)feature_count;
                    PPF_CNER = CNER / (double)feature_count;
                    PPF_Kappa = Kappa / (double)feature_count;
                    PPF_Overlap = Overlap / (double)feature_count;
                    PPF_RND_ACC = RND_ACC / (double)feature_count;
                    PPF_Support = Support / (double)feature_count;
                    PPF_BaseRate = BaseRate / (double)feature_count;
                    PPF_Youden = Youden / (double)feature_count;
                    PPF_NPV = NPV / (double)feature_count;
                    PPF_FNR = FNR / (double)feature_count;
                    PPF_FPR = FPR / (double)feature_count;
                    PPF_FDR = FDR / (double)feature_count;
                    PPF_FOR = FOR / (double)feature_count;
                    PPF_ACC = ACC / (double)feature_count;
                    PPF_GM = GM / (double)feature_count;
                    PPF_F1S = F1S / (double)feature_count;
                    PPF_G1S = G1S / (double)feature_count;
                    PPF_MCC = MCC / (double)feature_count;
                    PPF_BM_ = BM_ / (double)feature_count;
                    PPF_MK_ = MK_ / (double)feature_count;
                    PPF_BAC = BAC / (double)feature_count;
                    PPF_ROC_AUC_Approx_All = ROC_AUC_Approx_All / (double)feature_count;
                    PPF_ROC_AUC_Approx_11p = ROC_AUC_Approx_11p / (double)feature_count;
                    PPF_ROC_AUC_All = ROC_AUC_All / (double)feature_count;
                    //PPF_ROC_AUC_11p = ROC_AUC_11p / (double)feature_count;
                    PPF_PR_AUC_Approx_All = PR_AUC_Approx_All / (double)feature_count;
                    PPF_PR_AUC_Approx_11p = PR_AUC_Approx_11p / (double)feature_count;
                    PPF_PRI_AUC_Approx_All = PRI_AUC_Approx_All / (double)feature_count;
                    PPF_PRI_AUC_Approx_11p = PRI_AUC_Approx_11p / (double)feature_count;
                    PPF_AP_All = AP_All / (double)feature_count;
                    PPF_AP_11p = AP_11p / (double)feature_count;
                    PPF_API_All = API_All / (double)feature_count;
                    PPF_API_11p = API_11p / (double)feature_count;
                    PPF_Brier_All = Brier_All / (double)feature_count;
                    PPF_LRP = LRP / (double)feature_count;
                    PPF_LRN = LRN / (double)feature_count;
                    PPF_F1B_00 = F1B_00 / (double)feature_count;
                    PPF_F1B_01 = F1B_01 / (double)feature_count;
                    PPF_F1B_02 = F1B_02 / (double)feature_count;
                    PPF_F1B_03 = F1B_03 / (double)feature_count;
                    PPF_F1B_04 = F1B_04 / (double)feature_count;
                    PPF_F1B_05 = F1B_05 / (double)feature_count;
                    PPF_F1B_06 = F1B_06 / (double)feature_count;
                    PPF_F1B_07 = F1B_07 / (double)feature_count;
                    PPF_F1B_08 = F1B_08 / (double)feature_count;
                    PPF_F1B_09 = F1B_09 / (double)feature_count;
                    PPF_F1B_10 = F1B_10 / (double)feature_count;
                }

                if (group_count != 0)
                {
                    PPG_TP = TP / (double)group_count;
                    PPG_FP = FP / (double)group_count;
                    PPG_TN = TN / (double)group_count;
                    PPG_FN = FN / (double)group_count;
                    PPG_TPR = TPR / (double)group_count;
                    PPG_TNR = TNR / (double)group_count;
                    PPG_PPV = PPV / (double)group_count;
                    PPG_Precision = Precision / (double)group_count;
                    PPG_Prevalence = Prevalence / (double)group_count;
                    PPG_MCR = MCR / (double)group_count;
                    PPG_ER = ER / (double)group_count;
                    PPG_NER = NER / (double)group_count;
                    PPG_CNER = CNER / (double)group_count;
                    PPG_Kappa = Kappa / (double)group_count;
                    PPG_Overlap = Overlap / (double)group_count;
                    PPG_RND_ACC = RND_ACC / (double)group_count;
                    PPG_Support = Support / (double)group_count;
                    PPG_BaseRate = BaseRate / (double)group_count;
                    PPG_Youden = Youden / (double)group_count;
                    PPG_NPV = NPV / (double)group_count;
                    PPG_FNR = FNR / (double)group_count;
                    PPG_FPR = FPR / (double)group_count;
                    PPG_FDR = FDR / (double)group_count;
                    PPG_FOR = FOR / (double)group_count;
                    PPG_ACC = ACC / (double)group_count;
                    PPG_GM = GM / (double)group_count;
                    PPG_F1S = F1S / (double)group_count;
                    PPG_G1S = G1S / (double)group_count;
                    PPG_MCC = MCC / (double)group_count;
                    PPG_BM_ = BM_ / (double)group_count;
                    PPG_MK_ = MK_ / (double)group_count;
                    PPG_BAC = BAC / (double)group_count;
                    PPG_ROC_AUC_Approx_All = ROC_AUC_Approx_All / (double)group_count;
                    PPG_ROC_AUC_Approx_11p = ROC_AUC_Approx_11p / (double)group_count;
                    PPG_ROC_AUC_All = ROC_AUC_All / (double)group_count;
                    //PPG_ROC_AUC_11p = ROC_AUC_11p / (double)group_count;
                    PPG_PR_AUC_Approx_All = PR_AUC_Approx_All / (double)group_count;
                    PPG_PR_AUC_Approx_11p = PR_AUC_Approx_11p / (double)group_count;
                    PPG_PRI_AUC_Approx_All = PRI_AUC_Approx_All / (double)group_count;
                    PPG_PRI_AUC_Approx_11p = PRI_AUC_Approx_11p / (double)group_count;
                    PPG_AP_All = AP_All / (double)group_count;
                    PPG_AP_11p = AP_11p / (double)group_count;
                    PPG_API_All = API_All / (double)group_count;
                    PPG_API_11p = API_11p / (double)group_count;
                    PPG_Brier_All = Brier_All / (double)group_count;
                    PPG_LRP = LRP / (double)group_count;
                    PPG_LRN = LRN / (double)group_count;
                    PPG_F1B_00 = F1B_00 / (double)group_count;
                    PPG_F1B_01 = F1B_01 / (double)group_count;
                    PPG_F1B_02 = F1B_02 / (double)group_count;
                    PPG_F1B_03 = F1B_03 / (double)group_count;
                    PPG_F1B_04 = F1B_04 / (double)group_count;
                    PPG_F1B_05 = F1B_05 / (double)group_count;
                    PPG_F1B_06 = F1B_06 / (double)group_count;
                    PPG_F1B_07 = F1B_07 / (double)group_count;
                    PPG_F1B_08 = F1B_08 / (double)group_count;
                    PPG_F1B_09 = F1B_09 / (double)group_count;
                    PPG_F1B_10 = F1B_10 / (double)group_count;
                }

            }

            public void calculate_metrics(bool calculate_auc, List<prediction> prediction_list)
            {
                //var cm = this;

                if (calculate_auc && prediction_list != null && prediction_list.Count > 0 && prediction_list.Any(a=>a.probability_estimates_stated))
                {

                    var (p_roc_auc_approx_all, p_roc_auc_actual_all, p_pr_auc_approx_all, p_pri_auc_approx_all, p_ap_all, p_api_all, p_roc_xy_all, p_pr_xy_all, p_pri_xy_all) = performance_measure.Calculate_ROC_PR_AUC(prediction_list, class_id.Value, threshold_type.all_thresholds);
                    var (p_roc_auc_approx_11p, p_roc_auc_actual_11p, p_pr_auc_approx_11p, p_pri_auc_approx_11p, p_ap_11p, p_api_11p, p_roc_xy_11p, p_pr_xy_11p, p_pri_xy_11p) = performance_measure.Calculate_ROC_PR_AUC(prediction_list, class_id.Value, threshold_type.eleven_points);

                    var p_brier_score_all = brier(prediction_list, class_id.Value);
                    //var (p_brier_score_all, p_roc_auc_all, p_roc_auc2_all, p_pr_auc_all, p_pri_auc_all, p_ap_all, p_api_all, p_roc_xy_all, p_pr_xy_all, p_pri_xy_all) = performance_measure.Calculate_ROC_PR_AUC(prediction_list, class_id.Value, false);
                    //var (p_brier_score_11p, p_roc_auc_11p, p_roc_auc2_11p, p_pr_auc_11p, p_pri_auc_11p, p_ap_11p, p_api_11p, p_roc_xy_11p, p_pr_xy_11p, p_pri_xy_11p) = performance_measure.Calculate_ROC_PR_AUC(prediction_list, class_id.Value, true);

                    Brier_All = p_brier_score_all;
                    //Brier_11p = p_brier_score_11p;

                    ROC_AUC_Approx_All = p_roc_auc_approx_all;
                    ROC_AUC_Approx_11p = p_roc_auc_approx_11p;

                    ROC_AUC_All = p_roc_auc_actual_all;
                    //ROC_AUC_11p = p_roc_auc_actual_11p;

                    PR_AUC_Approx_All = p_pr_auc_approx_all;
                    PR_AUC_Approx_11p = p_pr_auc_approx_11p;

                    PRI_AUC_Approx_All = p_pri_auc_approx_all;
                    PRI_AUC_Approx_11p = p_pri_auc_approx_11p;

                    AP_All = p_ap_all;
                    AP_11p = p_ap_11p;
                    API_All = p_api_all;
                    API_11p = p_api_11p;

                    // PR (x: a.TPR, y: a.PPV)
                    // ROC (x: a.FPR, y: a.TPR)

                    roc_xy_str_all = p_roc_xy_all != null ? $"FPR;TPR/{string.Join("/", p_roc_xy_all.Select(a => $"{Math.Round(a.x, 6)};{Math.Round(a.y, 6)}").ToList())}" : "";
                    roc_xy_str_11p = p_roc_xy_11p != null ? $"FPR;TPR/{string.Join("/", p_roc_xy_11p.Select(a => $"{Math.Round(a.x, 6)};{Math.Round(a.y, 6)}").ToList())}" : "";

                    pr_xy_str_all = p_pr_xy_all != null ? $"TPR;PPV/{string.Join("/", p_pr_xy_all.Select(a => $"{Math.Round(a.x, 6)};{Math.Round(a.y, 6)}").ToList())}" : "";
                    pr_xy_str_11p = p_pr_xy_11p != null ? $"TPR;PPV/{string.Join("/", p_pr_xy_11p.Select(a => $"{Math.Round(a.x, 6)};{Math.Round(a.y, 6)}").ToList())}" : "";

                    pri_xy_str_all = p_pri_xy_all != null ? $"TPR;PPV/{string.Join("/", p_pri_xy_all.Select(a => $"{Math.Round(a.x, 6)};{Math.Round(a.y, 6)}").ToList())}" : "";
                    pri_xy_str_11p = p_pri_xy_11p != null ? $"TPR;PPV/{string.Join("/", p_pri_xy_11p.Select(a => $"{Math.Round(a.x, 6)};{Math.Round(a.y, 6)}").ToList())}" : "";


                }

                Support = (N) == (double)0.0 ? (double)0.0 : (double)(TP + FP) / (double)(N);
                BaseRate = (N) == (double)0.0 ? (double)0.0 : (double)(TP + FN) / (double)(N);

                Prevalence = (P + N) == (double)0.0 ? (double)0.0 : (FN + TP) / (P + N);
                MCR = (P + N) == (double)0.0 ? (double)0.0 : (FP + FN) / (P + N);
                TPR = (TP + FN) == (double)0.0 ? (double)0.0 : (double)(TP) / (double)(TP + FN);
                TNR = (TN + FP) == (double)0.0 ? (double)0.0 : (double)(TN) / (double)(TN + FP);
                Precision = (TP + FP) == (double)0.0 ? (double)0.0 : (double)(TP) / (double)(TP + FP);

                Overlap = (TP + FP + FN) == (double)0.0 ? (double)0.0 : TP / (TP + FP + FN);


                NER = (P + N) == (double)0.0 ? (double)0.0 : (P > N ? P : N) / (P + N);
                CNER = (P + N) == (double)0.0 ? (double)0.0 : P / (P + N);


                PPV = (TPR * Prevalence + (1.0 - TNR) * (1.0 - Prevalence)) == (double)0.0 ? (double)0.0 : (TPR * Prevalence) / (TPR * Prevalence + (1.0 - TNR) * (1.0 - Prevalence));

                NPV = (TN + FN) == (double)0.0 ? (double)0.0 : (double)(TN) / (double)(TN + FN);
                FNR = (FN + TP) == (double)0.0 ? (double)0.0 : (double)(FN) / (double)(FN + TP);
                FPR = (FP + TN) == (double)0.0 ? (double)0.0 : (double)(FP) / (double)(FP + TN);
                FDR = (FP + TP) == (double)0.0 ? (double)0.0 : (double)(FP) / (double)(FP + TP);
                FOR = (FN + TN) == (double)0.0 ? (double)0.0 : (double)(FN) / (double)(FN + TN);

                ACC = (P + N) == (double)0.0 ? (double)0.0 : (double)(TP + TN) / (double)(P + N);

                ER = 1.0 - ACC;
                Youden = TPR + TNR - 1.0;


                //Kappa = (totalAccuracy - randomAccuracy) / (1 - randomAccuracy)
                //totalAccuracy = (TP + TN) / (TP + TN + FP + FN)
                //randomAccuracy = referenceLikelihood(F) * resultLikelihood(F) + referenceLikelihood(T) * resultLikelihood(T)
                //randomAccuracy = (ActualFalse * PredictedFalse + ActualTrue * PredictedTrue) / Total * Total
                //randomAccuracy = (TN + FP) * (TN + FN) + (FN + TP) * (FP + TP) / Total * Total

                RND_ACC = ((TN + FP) * (TN + FN) + (FN + TP) * (FP + TP)) / ((TP + TN + FP + FN) * (TP + TN + FP + FN));

                Kappa = (1 - RND_ACC) == (double)0.0 ? (double)0.0 : (ACC - RND_ACC) / (1 - RND_ACC);

                GM = Math.Sqrt(TPR * TNR);
                F1S = (PPV + TPR) == (double)0.0 ? (double)0.0 : (double)(2 * PPV * TPR) / (double)(PPV + TPR);
                G1S = (PPV + TPR) == (double)0.0 ? (double)0.0 : (double)Math.Sqrt((double)(PPV * TPR));
                MCC = ((double)Math.Sqrt((double)((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN))) == (double)0.0) ? (double)0.0 : ((TP * TN) - (FP * FN)) / (double)Math.Sqrt((double)((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN)));
                BM_ = (TPR + TNR) - (double)1.0;
                MK_ = (PPV + NPV) - (double)1.0;
                BAC = (double)(TPR + TNR) / (double)2.0;

                LRP = (1 - TNR) == (double)0.0 ? (double)0.0 : (TPR) / (1.0 - TNR);
                LRN = (TNR) == (double)0.0 ? (double)0.0 : (1.0 - TPR) / (TNR);


                F1B_00 = fbeta2(PPV, TPR, (double)0.0);
                F1B_01 = fbeta2(PPV, TPR, (double)0.1);
                F1B_02 = fbeta2(PPV, TPR, (double)0.2);
                F1B_03 = fbeta2(PPV, TPR, (double)0.3);
                F1B_04 = fbeta2(PPV, TPR, (double)0.4);
                F1B_05 = fbeta2(PPV, TPR, (double)0.5);
                F1B_06 = fbeta2(PPV, TPR, (double)0.6);
                F1B_07 = fbeta2(PPV, TPR, (double)0.7);
                F1B_08 = fbeta2(PPV, TPR, (double)0.8);
                F1B_09 = fbeta2(PPV, TPR, (double)0.9);
                F1B_10 = fbeta2(PPV, TPR, (double)1.0);

                calculate_ppf();
            }

            public static confusion_matrix Average2(List<confusion_matrix> cm)
            {

                int? cid = cm.Select(a => a.class_id).Distinct().Count() == 1 ? cm.First().class_id : null;

                var result = new confusion_matrix()
                {
                    class_id = cid,
                    prediction_threshold = null,
                    prediction_threshold_class = null,
                    thresholds = cm.SelectMany(a => a.thresholds).Distinct().OrderByDescending(a => a).ToList(),

                    duration_grid_search = String.Join(";", cm.Select(a => a.duration_grid_search).Distinct().ToList()),
                    duration_nm_search = String.Join(";", cm.Select(a => a.duration_nm_search).Distinct().ToList()),
                    duration_training = String.Join(";", cm.Select(a => a.duration_training).Distinct().ToList()),
                    duration_testing = String.Join(";", cm.Select(a => a.duration_testing).Distinct().ToList()),


                    class_weight = cm.Select(a => a.class_weight).Average(),
                    class_name = String.Join(";", cm.Select(a => a.class_name).Distinct().ToList()),

                    testing_set_name = String.Join(";", cm.Select(a => a.testing_set_name).Distinct().ToList()),
                    //experiment_id1 = String.Join(";", cm.Select(a => a.experiment_id1).Distinct().ToList()),
                    //experiment_id2 = String.Join(";", cm.Select(a => a.experiment_id2).Distinct().ToList()),
                    //experiment_id3 = String.Join(";", cm.Select(a => a.experiment_id3).Distinct().ToList()),

                    //feature_selection_type = String.Join(";", cm.Select(a => a.feature_selection_type).Distinct().ToList()),
                    
                    //fs_perf_selection = String.Join(";", cm.Select(a => a.fs_perf_selection).Distinct().ToList()),


                    features_included = String.Join(";", cm.Select(a => a.features_included).Distinct().ToList()),
                    kernel = String.Join(";", cm.Select(a => a.kernel).Distinct().ToList()),


                    libsvm_cv = cm.Select(a => a.libsvm_cv).Average(),
                    libsvm_cv_precision = cm.Select(a => a.libsvm_cv_precision).Average(),
                    libsvm_cv_accuracy = cm.Select(a => a.libsvm_cv_accuracy).Average(),
                    libsvm_cv_fscore = cm.Select(a => a.libsvm_cv_fscore).Average(),
                    libsvm_cv_ap = cm.Select(a => a.libsvm_cv_ap).Average(),
                    libsvm_cv_auc = cm.Select(a => a.libsvm_cv_auc).Average(),
                    libsvm_cv_bac = cm.Select(a => a.libsvm_cv_bac).Average(),
                    libsvm_cv_recall = cm.Select(a => a.libsvm_cv_recall).Average(),


                    //math_operation = String.Join(";", cm.Select(a => a.math_operation).Distinct().ToList()),

                    pr_xy_str_all = "",
                    pr_xy_str_11p = "",
                    pri_xy_str_all = "",
                    pri_xy_str_11p = "",
                    roc_xy_str_all = "",
                    roc_xy_str_11p = "",

                    scaling_method = String.Join(";", cm.Select(a => a.scaling_method).Distinct().ToList()),
                    svm_type = String.Join(";", cm.Select(a => a.svm_type).Distinct().ToList()),
                    training_resampling_method = String.Join(";", cm.Select(a => a.training_resampling_method).Distinct().ToList()),
                    kernel_parameter_search_method = String.Join(";", cm.Select(a => a.kernel_parameter_search_method).Distinct().ToList()),



                    P = cm.Select(a => a.P).Average(),
                    N = cm.Select(a => a.N).Average(),

                    TP = cm.Select(a => a.TP).Average(),
                    FP = cm.Select(a => a.FP).Average(),
                    TN = cm.Select(a => a.TN).Average(),
                    FN = cm.Select(a => a.FN).Average(),
                    TPR = cm.Select(a => a.TPR).Average(),
                    TNR = cm.Select(a => a.TNR).Average(),
                    PPV = cm.Select(a => a.PPV).Average(),

                    Precision = cm.Select(a => a.Precision).Average(),
                    Prevalence = cm.Select(a => a.Prevalence).Average(),
                    MCR = cm.Select(a => a.MCR).Average(),
                    ER = cm.Select(a => a.ER).Average(),
                    NER = cm.Select(a => a.NER).Average(),
                    CNER = cm.Select(a => a.CNER).Average(),
                    Kappa = cm.Select(a => a.Kappa).Average(),
                    Overlap = cm.Select(a => a.Overlap).Average(),
                    RND_ACC = cm.Select(a => a.RND_ACC).Average(),
                    Support = cm.Select(a => a.Support).Average(),
                    BaseRate = cm.Select(a => a.BaseRate).Average(),
                    Youden = cm.Select(a => a.Youden).Average(),


                    NPV = cm.Select(a => a.NPV).Average(),
                    FNR = cm.Select(a => a.FNR).Average(),
                    FPR = cm.Select(a => a.FPR).Average(),
                    FDR = cm.Select(a => a.FDR).Average(),
                    FOR = cm.Select(a => a.FOR).Average(),
                    ACC = cm.Select(a => a.ACC).Average(),
                    GM = cm.Select(a => a.GM).Average(),
                    F1S = cm.Select(a => a.F1S).Average(),
                    G1S = cm.Select(a => a.G1S).Average(),
                    MCC = cm.Select(a => a.MCC).Average(),
                    BM_ = cm.Select(a => a.BM_).Average(),
                    MK_ = cm.Select(a => a.MK_).Average(),
                    BAC = cm.Select(a => a.BAC).Average(),
                    ROC_AUC_Approx_All = cm.Select(a => a.ROC_AUC_Approx_All).Average(),
                    ROC_AUC_Approx_11p = cm.Select(a => a.ROC_AUC_Approx_11p).Average(),
                    PR_AUC_Approx_All = cm.Select(a => a.PR_AUC_Approx_All).Average(),
                    PR_AUC_Approx_11p = cm.Select(a => a.PR_AUC_Approx_11p).Average(),
                    PRI_AUC_Approx_All = cm.Select(a => a.PRI_AUC_Approx_All).Average(),
                    PRI_AUC_Approx_11p = cm.Select(a => a.PRI_AUC_Approx_11p).Average(),
                    ROC_AUC_All = cm.Select(a => a.ROC_AUC_All).Average(),

                    AP_All = cm.Select(a => a.AP_All).Average(),
                    AP_11p = cm.Select(a => a.AP_11p).Average(),
                    API_All = cm.Select(a => a.API_All).Average(),
                    API_11p = cm.Select(a => a.API_11p).Average(),
                    Brier_All = cm.Select(a => a.Brier_All).Average(),
                    F1B_00 = cm.Select(a => a.F1B_00).Average(),
                    F1B_01 = cm.Select(a => a.F1B_01).Average(),
                    F1B_02 = cm.Select(a => a.F1B_02).Average(),
                    F1B_03 = cm.Select(a => a.F1B_03).Average(),
                    F1B_04 = cm.Select(a => a.F1B_04).Average(),
                    F1B_05 = cm.Select(a => a.F1B_05).Average(),
                    F1B_06 = cm.Select(a => a.F1B_06).Average(),
                    F1B_07 = cm.Select(a => a.F1B_07).Average(),
                    F1B_08 = cm.Select(a => a.F1B_08).Average(),
                    F1B_09 = cm.Select(a => a.F1B_09).Average(),
                    F1B_10 = cm.Select(a => a.F1B_10).Average(),
                    LRN = cm.Select(a => a.LRN).Average(),
                    LRP = cm.Select(a => a.LRP).Average(),

                    randomisation_cv_index = cm.Select(a => a.randomisation_cv_index).Average(),
                    randomisation_cv_folds = cm.Select(a => a.randomisation_cv_folds).Average(),


                    outer_cv_folds = cm.Select(a => a.outer_cv_folds).Average(),
                    inner_cv_folds = cm.Select(a => a.inner_cv_folds).Average(),
                    class_size = cm.Select(a => a.class_size).Average(),
                    class_training_size = cm.Select(a => a.class_training_size).Average(),
                    feature_count = (int)cm.Select(a => a.feature_count).Average(),
                    group_count = (int)cm.Select(a => a.group_count).Average(),

                    cost = cm.Select(a => a.cost).Average(),
                    gamma = cm.Select(a => a.gamma).Average(),
                    epsilon = cm.Select(a => a.epsilon).Average(),
                    coef0 = cm.Select(a => a.coef0).Average(),
                    degree = cm.Select(a => a.degree).Average(),

                    
                    outer_cv_index = cm.Select(a => a.outer_cv_index).Average(),


                    testing_size_pct = cm.Select(a => a.testing_size_pct).Average(),
                    training_size_pct = cm.Select(a => a.training_size_pct).Average(),
                    unused_size_pct = cm.Select(a => a.unused_size_pct).Average(),

                };


                result.calculate_metrics(false, null);
                result.calculate_ppf();

                return result;
            }

            public static List<confusion_matrix> Average1(List<confusion_matrix> confusion_matrices)//, bool class_specific = true)
            {
                //var print_params = true;

                //if (print_params)
                //{
                //    var param_list = new List<(string key, string value)>() {(nameof(confusion_matrices), confusion_matrices.ToString()),};


                //    if (program.write_console_log) program.WriteLine($@"{nameof(Average1)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
                //}

                var x = confusion_matrices.GroupBy(a =>
                    (

                        a.svm_type,
                        a.kernel,
                        a.training_resampling_method,
                        a.randomisation_cv_folds,
                        a.outer_cv_folds,
                        a.inner_cv_folds,
                        a.kernel_parameter_search_method,
                        
                        a.testing_set_name,
                        //a.experiment_id1,
                        //a.experiment_id2,
                        //a.experiment_id3,
                        
                        
                        a.class_id,
                        a.class_name,
                        a.feature_count,
                        a.group_count,
                        a.features_included,
                        //a.math_operation,
                        a.scaling_method,
                        a.training_size_pct,
                        a.unused_size_pct,
                        a.testing_size_pct,
                        a.class_size,
                        a.class_training_size,
                        a.prediction_threshold_class,
                        a.prediction_threshold
                        )).Select(cm =>
                {
                    var y = new confusion_matrix()
                    {
                        class_id = cm.Key.class_id,
                        prediction_threshold = cm.Key.prediction_threshold,
                        prediction_threshold_class = cm.Key.prediction_threshold_class,
                        thresholds = cm.SelectMany(a => a.thresholds).Distinct().OrderByDescending(a => a).ToList(),

                        duration_grid_search = String.Join(";", cm.Select(a => a.duration_grid_search).Distinct().ToList()),
                        duration_nm_search = String.Join(";", cm.Select(a => a.duration_nm_search).Distinct().ToList()),
                        duration_training = String.Join(";", cm.Select(a => a.duration_training).Distinct().ToList()),
                        duration_testing = String.Join(";", cm.Select(a => a.duration_testing).Distinct().ToList()),


                        class_weight = cm.Select(a => a.class_weight).Average(),
                        class_name = String.Join(";", cm.Select(a => a.class_name).Distinct().ToList()),

                        testing_set_name = String.Join(";", cm.Select(a => a.testing_set_name).Distinct().ToList()),
                        //experiment_id1 = String.Join(";", cm.Select(a => a.experiment_id1).Distinct().ToList()),
                        //experiment_id2 = String.Join(";", cm.Select(a => a.experiment_id2).Distinct().ToList()),
                        //experiment_id3 = String.Join(";", cm.Select(a => a.experiment_id3).Distinct().ToList()),
                        
                        ////fs_starting_point = String.Join(";", cm.Select(a => a.fs_starting_point).Distinct().ToList()),
                        //feature_selection_type = String.Join(";", cm.Select(a => a.feature_selection_type).Distinct().ToList()),
                        ////fs_algorithm_direction = String.Join(";", cm.Select(a => a.fs_algorithm_direction).Distinct().ToList()),
                        //fs_perf_selection = String.Join(";", cm.Select(a => a.fs_perf_selection).Distinct().ToList()),

                        
                        features_included = String.Join(";", cm.Select(a => a.features_included).Distinct().ToList()),
                        kernel = String.Join(";", cm.Select(a => a.kernel).Distinct().ToList()),


                        libsvm_cv =  cm.Select(a => a.libsvm_cv).Average(),
                        libsvm_cv_precision =  cm.Select(a => a.libsvm_cv_precision).Average(),
                        libsvm_cv_accuracy = cm.Select(a => a.libsvm_cv_accuracy).Average(),
                        libsvm_cv_fscore =  cm.Select(a => a.libsvm_cv_fscore).Average(),
                        libsvm_cv_ap =  cm.Select(a => a.libsvm_cv_ap).Average(),
                        libsvm_cv_auc =  cm.Select(a => a.libsvm_cv_auc).Average(),
                        libsvm_cv_bac = cm.Select(a => a.libsvm_cv_bac).Average(),
                        libsvm_cv_recall = cm.Select(a => a.libsvm_cv_recall).Average(),


                        //math_operation = String.Join(";", cm.Select(a => a.math_operation).Distinct().ToList()),

                        pr_xy_str_all = "",
                        pr_xy_str_11p = "",
                        pri_xy_str_all = "",
                        pri_xy_str_11p = "",
                        roc_xy_str_all = "",
                        roc_xy_str_11p = "",

                        scaling_method = String.Join(";", cm.Select(a => a.scaling_method).Distinct().ToList()),
                        svm_type = String.Join(";", cm.Select(a => a.svm_type).Distinct().ToList()),
                        training_resampling_method = String.Join(";", cm.Select(a => a.training_resampling_method).Distinct().ToList()),
                        kernel_parameter_search_method = String.Join(";", cm.Select(a => a.kernel_parameter_search_method).Distinct().ToList()),



                        P = cm.Select(a => a.P).Average(),
                        N = cm.Select(a => a.N).Average(),

                        TP = cm.Select(a => a.TP).Average(),
                        FP = cm.Select(a => a.FP).Average(),
                        TN = cm.Select(a => a.TN).Average(),
                        FN = cm.Select(a => a.FN).Average(),
                        TPR = cm.Select(a => a.TPR).Average(),
                        TNR = cm.Select(a => a.TNR).Average(),
                        PPV = cm.Select(a => a.PPV).Average(),

                        Precision = cm.Select(a => a.Precision).Average(),
                        Prevalence = cm.Select(a => a.Prevalence).Average(),
                        MCR = cm.Select(a => a.MCR).Average(),
                        ER = cm.Select(a => a.ER).Average(),
                        NER = cm.Select(a => a.NER).Average(),
                        CNER = cm.Select(a => a.CNER).Average(),
                        Kappa = cm.Select(a => a.Kappa).Average(),
                        Overlap = cm.Select(a => a.Overlap).Average(),
                        RND_ACC = cm.Select(a => a.RND_ACC).Average(),
                        Support = cm.Select(a => a.Support).Average(),
                        BaseRate = cm.Select(a => a.BaseRate).Average(),
                        Youden = cm.Select(a => a.Youden).Average(),
                         

                        NPV = cm.Select(a => a.NPV).Average(),
                        FNR = cm.Select(a => a.FNR).Average(),
                        FPR = cm.Select(a => a.FPR).Average(),
                        FDR = cm.Select(a => a.FDR).Average(),
                        FOR = cm.Select(a => a.FOR).Average(),
                        ACC = cm.Select(a => a.ACC).Average(),
                        GM = cm.Select(a => a.GM).Average(),
                        F1S = cm.Select(a => a.F1S).Average(),
                        G1S = cm.Select(a => a.G1S).Average(),
                        MCC = cm.Select(a => a.MCC).Average(),
                        BM_ = cm.Select(a => a.BM_).Average(),
                        MK_ = cm.Select(a => a.MK_).Average(),
                        BAC = cm.Select(a => a.BAC).Average(),
                        ROC_AUC_Approx_All = cm.Select(a => a.ROC_AUC_Approx_All).Average(),
                        ROC_AUC_Approx_11p = cm.Select(a => a.ROC_AUC_Approx_11p).Average(),
                        PR_AUC_Approx_All = cm.Select(a => a.PR_AUC_Approx_All).Average(),
                        PR_AUC_Approx_11p = cm.Select(a => a.PR_AUC_Approx_11p).Average(),
                        PRI_AUC_Approx_All = cm.Select(a => a.PRI_AUC_Approx_All).Average(),
                        PRI_AUC_Approx_11p = cm.Select(a => a.PRI_AUC_Approx_11p).Average(),
                        ROC_AUC_All = cm.Select(a => a.ROC_AUC_All).Average(),
                        
                        AP_All = cm.Select(a => a.AP_All).Average(),
                        AP_11p = cm.Select(a => a.AP_11p).Average(),
                        API_All = cm.Select(a => a.API_All).Average(),
                        API_11p = cm.Select(a => a.API_11p).Average(),
                        Brier_All = cm.Select(a => a.Brier_All).Average(),
                        F1B_00 = cm.Select(a => a.F1B_00).Average(),
                        F1B_01 = cm.Select(a => a.F1B_01).Average(),
                        F1B_02 = cm.Select(a => a.F1B_02).Average(),
                        F1B_03 = cm.Select(a => a.F1B_03).Average(),
                        F1B_04 = cm.Select(a => a.F1B_04).Average(),
                        F1B_05 = cm.Select(a => a.F1B_05).Average(),
                        F1B_06 = cm.Select(a => a.F1B_06).Average(),
                        F1B_07 = cm.Select(a => a.F1B_07).Average(),
                        F1B_08 = cm.Select(a => a.F1B_08).Average(),
                        F1B_09 = cm.Select(a => a.F1B_09).Average(),
                        F1B_10 = cm.Select(a => a.F1B_10).Average(),
                        LRN = cm.Select(a => a.LRN).Average(),
                        LRP = cm.Select(a => a.LRP).Average(),

                        randomisation_cv_index = cm.Select(a => a.randomisation_cv_index).Average(),
                        randomisation_cv_folds = cm.Select(a => a.randomisation_cv_folds).Average(),


                        outer_cv_folds = cm.Select(a => a.outer_cv_folds).Average(),
                        inner_cv_folds = cm.Select(a => a.inner_cv_folds).Average(),
                        class_size = cm.Select(a => a.class_size).Average(),
                        class_training_size = cm.Select(a => a.class_training_size).Average(),
                        feature_count = (int)cm.Select(a => a.feature_count).Average(),
                        group_count = (int)cm.Select(a => a.group_count).Average(),

                        cost = cm.Select(a => a.cost).Average(),
                        gamma = cm.Select(a => a.gamma).Average(),
                        epsilon = cm.Select(a => a.epsilon).Average(),
                        coef0 = cm.Select(a => a.coef0).Average(),
                        degree = cm.Select(a => a.degree).Average(),

                        
                        outer_cv_index = cm.Select(a => a.outer_cv_index).Average(),


                        testing_size_pct = cm.Select(a => a.testing_size_pct).Average(),
                        training_size_pct = cm.Select(a => a.training_size_pct).Average(),
                        unused_size_pct = cm.Select(a => a.unused_size_pct).Average(),
                        

                    };

                    y.calculate_metrics(false, null);

                    return y;
                }).ToList();

                return x;
            }

            public static readonly string csv_header = string.Join(",", new string[]
            {
                nameof(testing_set_name),
                //nameof(experiment_id1),
                //nameof(experiment_id2),
                //nameof(experiment_id3),
                
                ////nameof(fs_starting_point),
                //nameof(feature_selection_type),
                ////nameof(fs_algorithm_direction),
                //nameof(fs_perf_selection),
                nameof(duration_grid_search),
                nameof(duration_nm_search),
                nameof(duration_training),
                nameof(duration_testing),
                //nameof(math_operation),
                nameof(scaling_method),

                nameof(libsvm_cv),
                nameof(libsvm_cv_accuracy),
                nameof(libsvm_cv_bac),
                nameof(libsvm_cv_recall),
                nameof(libsvm_cv_fscore),
                nameof(libsvm_cv_precision),
                nameof(libsvm_cv_auc),
                nameof(libsvm_cv_ap),

                nameof(prediction_threshold),
                nameof(prediction_threshold_class),
                nameof(feature_count),
                nameof(group_count),
                nameof(features_included),
                nameof(training_resampling_method),
                nameof(training_size_pct),
                nameof(unused_size_pct),
                nameof(testing_size_pct),
                nameof(inner_cv_folds),
                nameof(randomisation_cv_index),
                nameof(randomisation_cv_folds),
                nameof(outer_cv_index),
                nameof(outer_cv_folds),
                nameof(svm_type),
                nameof(kernel),
                nameof(kernel_parameter_search_method),
                nameof(cost),
                nameof(gamma),
                nameof(epsilon),
                nameof(coef0),
                nameof(degree),
                
                nameof(class_id),
                nameof(class_name),
                nameof(class_size),
                nameof(class_training_size),
                nameof(P),
                nameof(N),
                nameof(TP),
                nameof(FP),
                nameof(TN),
                nameof(FN),
                nameof(TPR),
                nameof(TNR),
                nameof(PPV),
                nameof(Precision),
                nameof(Prevalence),
                nameof(MCR),
                nameof(ER),
                nameof(NER),
                nameof(CNER),
                nameof(Kappa),
                nameof(Overlap),
                nameof(RND_ACC),
                nameof(Support),
                nameof(BaseRate),
                nameof(Youden),
                nameof(NPV),
                nameof(FNR),
                nameof(FPR),
                nameof(FDR),
                nameof(FOR),
                nameof(ACC),
                nameof(GM),
                nameof(F1S),
                nameof(G1S),
                nameof(MCC),
                nameof(BM_),
                nameof(MK_),
                nameof(BAC),
                nameof(ROC_AUC_Approx_All),
                nameof(ROC_AUC_Approx_11p),
                nameof(ROC_AUC_All),
                //nameof(ROC_AUC_11p),
                nameof(PR_AUC_Approx_All),
                nameof(PR_AUC_Approx_11p),
                nameof(PRI_AUC_Approx_All),
                nameof(PRI_AUC_Approx_11p),
                nameof(AP_All),
                nameof(AP_11p),
                nameof(API_All),
                nameof(API_11p),
                nameof(Brier_All),
                nameof(LRP),
                nameof(LRN),
                nameof(F1B_00),
                nameof(F1B_01),
                nameof(F1B_02),
                nameof(F1B_03),
                nameof(F1B_04),
                nameof(F1B_05),
                nameof(F1B_06),
                nameof(F1B_07),
                nameof(F1B_08),
                nameof(F1B_09),
                nameof(F1B_10),

                nameof(PPF_TP),
                nameof(PPF_FP),
                nameof(PPF_TN),
                nameof(PPF_FN),
                nameof(PPF_TPR),
                nameof(PPF_TNR),
                nameof(PPF_PPV),
                nameof(PPF_Precision),
                nameof(PPF_Prevalence),
                nameof(PPF_MCR),
                nameof(PPF_ER),
                nameof(PPF_NER),
                nameof(PPF_CNER),
                nameof(PPF_Kappa),
                nameof(PPF_Overlap),
                nameof(PPF_RND_ACC),
                nameof(PPF_Support),
                nameof(PPF_BaseRate),
                nameof(PPF_Youden),
                nameof(PPF_NPV),
                nameof(PPF_FNR),
                nameof(PPF_FPR),
                nameof(PPF_FDR),
                nameof(PPF_FOR),
                nameof(PPF_ACC),
                nameof(PPF_GM),
                nameof(PPF_F1S),
                nameof(PPF_G1S),
                nameof(PPF_MCC),
                nameof(PPF_BM_),
                nameof(PPF_MK_),
                nameof(PPF_BAC),
                nameof(PPF_ROC_AUC_Approx_All),
                nameof(PPF_ROC_AUC_Approx_11p),
                nameof(PPF_ROC_AUC_All),
                //nameof(PPF_ROC_AUC_11p),
                nameof(PPF_PR_AUC_Approx_All),
                nameof(PPF_PR_AUC_Approx_11p),
                nameof(PPF_PRI_AUC_Approx_All),
                nameof(PPF_PRI_AUC_Approx_11p),
                nameof(PPF_AP_All),
                nameof(PPF_AP_11p),
                nameof(PPF_API_All),
                nameof(PPF_API_11p),
                nameof(PPF_Brier_All),
                nameof(PPF_LRP),
                nameof(PPF_LRN),
                nameof(PPF_F1B_00),
                nameof(PPF_F1B_01),
                nameof(PPF_F1B_02),
                nameof(PPF_F1B_03),
                nameof(PPF_F1B_04),
                nameof(PPF_F1B_05),
                nameof(PPF_F1B_06),
                nameof(PPF_F1B_07),
                nameof(PPF_F1B_08),
                nameof(PPF_F1B_09),
                nameof(PPF_F1B_10),

                nameof(PPG_TP),
                nameof(PPG_FP),
                nameof(PPG_TN),
                nameof(PPG_FN),
                nameof(PPG_TPR),
                nameof(PPG_TNR),
                nameof(PPG_PPV),
                nameof(PPG_Precision),
                nameof(PPG_Prevalence),
                nameof(PPG_MCR),
                nameof(PPG_ER),
                nameof(PPG_NER),
                nameof(PPG_CNER),
                nameof(PPG_Kappa),
                nameof(PPG_Overlap),
                nameof(PPG_RND_ACC),
                nameof(PPG_Support),
                nameof(PPG_BaseRate),
                nameof(PPG_Youden),
                nameof(PPG_NPV),
                nameof(PPG_FNR),
                nameof(PPG_FPR),
                nameof(PPG_FDR),
                nameof(PPG_FOR),
                nameof(PPG_ACC),
                nameof(PPG_GM),
                nameof(PPG_F1S),
                nameof(PPG_G1S),
                nameof(PPG_MCC),
                nameof(PPG_BM_),
                nameof(PPG_MK_),
                nameof(PPG_BAC),
                nameof(PPG_ROC_AUC_Approx_All),
                nameof(PPG_ROC_AUC_Approx_11p),
                nameof(PPG_ROC_AUC_All),
                //nameof(PPG_ROC_AUC_11p),
                nameof(PPG_PR_AUC_Approx_All),
                nameof(PPG_PR_AUC_Approx_11p),
                nameof(PPG_PRI_AUC_Approx_All),
                nameof(PPG_PRI_AUC_Approx_11p),
                nameof(PPG_AP_All),
                nameof(PPG_AP_11p),
                nameof(PPG_API_All),
                nameof(PPG_API_11p),
                nameof(PPG_Brier_All),
                nameof(PPG_LRP),
                nameof(PPG_LRN),
                nameof(PPG_F1B_00),
                nameof(PPG_F1B_01),
                nameof(PPG_F1B_02),
                nameof(PPG_F1B_03),
                nameof(PPG_F1B_04),
                nameof(PPG_F1B_05),
                nameof(PPG_F1B_06),
                nameof(PPG_F1B_07),
                nameof(PPG_F1B_08),
                nameof(PPG_F1B_09),
                nameof(PPG_F1B_10),

                nameof(roc_xy_str_all),
                nameof(roc_xy_str_11p),
                nameof(pr_xy_str_all),
                nameof(pr_xy_str_11p),
                nameof(pri_xy_str_all),
                nameof(pri_xy_str_11p),
            });



            public override string ToString()
            {
                return String.Join(",", new List<object>()
                {





























                    testing_set_name,
                    //experiment_id1,
                    //experiment_id2,
                    //experiment_id3,
                    
                    ////fs_starting_point,
                    //feature_selection_type,
                    ////fs_algorithm_direction,
                    //fs_perf_selection,
                    duration_grid_search,
                    duration_nm_search,
                    duration_training,
                    duration_testing,                    
                    //math_operation,
                    scaling_method,

                    libsvm_cv,
                    libsvm_cv_accuracy,
                    libsvm_cv_bac,
                    libsvm_cv_recall,
                    libsvm_cv_fscore,
                    libsvm_cv_precision,
                    libsvm_cv_auc,
                    libsvm_cv_ap,

                    prediction_threshold,
                    prediction_threshold_class,
                    feature_count,
                    group_count,
                    features_included,
                    training_resampling_method,
                    training_size_pct,
                    unused_size_pct,
                    testing_size_pct,
                    inner_cv_folds,
                    randomisation_cv_index,
                    randomisation_cv_folds,
                    outer_cv_index,
                    outer_cv_folds,
                    svm_type,
                    kernel,
                    kernel_parameter_search_method,
                    cost,
                    gamma,
                    epsilon,
                    coef0,
                    degree,
                    
                    class_id,
                    class_name,
                    class_size,
                    class_training_size,
                    P,
                    N,
                    TP,
                    FP,
                    TN,
                    FN,
                    TPR,
                    TNR,
                    PPV,
                    Precision,
                    Prevalence,
                    MCR,
                    ER,
                    NER,
                    CNER,
                    Kappa,
                    Overlap,
                    RND_ACC,
                    Support,
                    BaseRate,
                    Youden,
                    NPV,
                    FNR,
                    FPR,
                    FDR,
                    FOR,
                    ACC,
                    GM,
                    F1S,
                    G1S,
                    MCC,
                    BM_,
                    MK_,
                    BAC,
                    ROC_AUC_Approx_All,
                    ROC_AUC_Approx_11p,
                    ROC_AUC_All,
                    //ROC_AUC_11p,
                    PR_AUC_Approx_All,
                    PR_AUC_Approx_11p,
                    PRI_AUC_Approx_All,
                    PRI_AUC_Approx_11p,
                    AP_All,
                    AP_11p,
                    API_All,
                    API_11p,
                    Brier_All,
                    LRP,
                    LRN,
                    F1B_00,
                    F1B_01,
                    F1B_02,
                    F1B_03,
                    F1B_04,
                    F1B_05,
                    F1B_06,
                    F1B_07,
                    F1B_08,
                    F1B_09,
                    F1B_10,
                    PPF_TP,
                    PPF_FP,
                    PPF_TN,
                    PPF_FN,
                    PPF_TPR,
                    PPF_TNR,
                    PPF_PPV,
                    PPF_Precision,
                    PPF_Prevalence,
                    PPF_MCR,
                    PPF_ER,
                    PPF_NER,
                    PPF_CNER,
                    PPF_Kappa,
                    PPF_Overlap,
                    PPF_RND_ACC,
                    PPF_Support,
                    PPF_BaseRate,
                    PPF_Youden,
                    PPF_NPV,
                    PPF_FNR,
                    PPF_FPR,
                    PPF_FDR,
                    PPF_FOR,
                    PPF_ACC,
                    PPF_GM,
                    PPF_F1S,
                    PPF_G1S,
                    PPF_MCC,
                    PPF_BM_,
                    PPF_MK_,
                    PPF_BAC,
                    PPF_ROC_AUC_Approx_All,
                    PPF_ROC_AUC_Approx_11p,
                    PPF_ROC_AUC_All,
                    //PPF_ROC_AUC_11p,
                    PPF_PR_AUC_Approx_All,
                    PPF_PR_AUC_Approx_11p,
                    PPF_PRI_AUC_Approx_All,
                    PPF_PRI_AUC_Approx_11p,
                    PPF_AP_All,
                    PPF_AP_11p,
                    PPF_API_All,
                    PPF_API_11p,
                    PPF_Brier_All,
                    PPF_LRP,
                    PPF_LRN,
                    PPF_F1B_00,
                    PPF_F1B_01,
                    PPF_F1B_02,
                    PPF_F1B_03,
                    PPF_F1B_04,
                    PPF_F1B_05,
                    PPF_F1B_06,
                    PPF_F1B_07,
                    PPF_F1B_08,
                    PPF_F1B_09,
                    PPF_F1B_10,

                    PPG_TP,
                    PPG_FP,
                    PPG_TN,
                    PPG_FN,
                    PPG_TPR,
                    PPG_TNR,
                    PPG_PPV,
                    PPG_Precision,
                    PPG_Prevalence,
                    PPG_MCR,
                    PPG_ER,
                    PPG_NER,
                    PPG_CNER,
                    PPG_Kappa,
                    PPG_Overlap,
                    PPG_RND_ACC,
                    PPG_Support,
                    PPG_BaseRate,
                    PPG_Youden,
                    PPG_NPV,
                    PPG_FNR,
                    PPG_FPR,
                    PPG_FDR,
                    PPG_FOR,
                    PPG_ACC,
                    PPG_GM,
                    PPG_F1S,
                    PPG_G1S,
                    PPG_MCC,
                    PPG_BM_,
                    PPG_MK_,
                    PPG_BAC,
                    PPG_ROC_AUC_Approx_All,
                    PPG_ROC_AUC_Approx_11p,
                    PPG_ROC_AUC_All,
                    //PPG_ROC_AUC_11p,
                    PPG_PR_AUC_Approx_All,
                    PPG_PR_AUC_Approx_11p,
                    PPG_PRI_AUC_Approx_All,
                    PPG_PRI_AUC_Approx_11p,
                    PPG_AP_All,
                    PPG_AP_11p,
                    PPG_API_All,
                    PPG_API_11p,
                    PPG_Brier_All,
                    PPG_LRP,
                    PPG_LRN,
                    PPG_F1B_00,
                    PPG_F1B_01,
                    PPG_F1B_02,
                    PPG_F1B_03,
                    PPG_F1B_04,
                    PPG_F1B_05,
                    PPG_F1B_06,
                    PPG_F1B_07,
                    PPG_F1B_08,
                    PPG_F1B_09,
                    PPG_F1B_10,

                    roc_xy_str_all,
                    roc_xy_str_11p,
                    pr_xy_str_all,
                    pr_xy_str_11p,
                    pri_xy_str_all,
                    pri_xy_str_11p,

                }.Select(a=>a?.ToString().Replace(",",";") ?? "").ToList());
            }
        }

        //public static double fbeta1(double PPV, double TPR, double fbeta)
        //{
        //    var fb = (PPV == 0.0 || TPR == 0.0) ? (double)0.0 : (double)(1 + (fbeta * fbeta)) * ((PPV * TPR) / (((fbeta * fbeta) * PPV) + TPR));

        //    return fb;
        //}

        public static double fbeta2(double PPV, double TPR, double fbeta)
        {
            var fb = (PPV == 0.0 || TPR == 0.0) ? (double)0.0 : (double)(1 / (fbeta * (1 / PPV) + (1 - fbeta) * (1 / TPR)));

            return fb;
        }

        public static List<confusion_matrix> count_prediction_error(
            List<prediction> prediction_list,
            double? threshold = null,
            int? threshold_class = null,
            bool calculate_auc = true
            )
        {
            
            //var param_list = new List<(string key, string value)>()
            //{
            //    (nameof(prediction_list),prediction_list.ToString()),
            //    (nameof(threshold),threshold.ToString()),
            //    (nameof(threshold_class),threshold_class.ToString()),
            //    (nameof(calculate_auc),calculate_auc.ToString()),
            //};

            //if (program.write_console_log) program.WriteLine($@"{nameof(count_prediction_error)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            

            var actual_class_id_list = prediction_list.Select(a => a.actual_class).Distinct().OrderBy(a => a).ToList();

            var confusion_matrix_list = new List<confusion_matrix>();

            for (var i = 0; i < actual_class_id_list.Count; i++)
            {
                var actual_class_id = actual_class_id_list[i];

                var confusion_matrix1 = new confusion_matrix()
                {
                    class_id = actual_class_id,
                    P = prediction_list.Count(b => actual_class_id == b.actual_class),
                    N = prediction_list.Count(b => actual_class_id != b.actual_class),
                    prediction_threshold = threshold,
                    prediction_threshold_class = threshold_class,
                    thresholds = prediction_list.Select(a => a.probability_estimates.FirstOrDefault(b => b.class_id == actual_class_id).probability_estimate).Distinct().OrderByDescending(a => a).ToList(),

                };
                confusion_matrix_list.Add(confusion_matrix1);

            }

            for (var prediction_list_index = 0; prediction_list_index < prediction_list.Count; prediction_list_index++)
            {
                var prediction = prediction_list[prediction_list_index];
                var actual_class = prediction.actual_class;
                var predicted_class = prediction.default_predicted_class;

                var actual_class_matrix = confusion_matrix_list.First(a => a.class_id == actual_class);
                var predicted_class_matrix = confusion_matrix_list.First(a => a.class_id == predicted_class);

                if (actual_class == predicted_class)
                {
                    actual_class_matrix.TP++;

                    for (var index = 0; index < confusion_matrix_list.Count; index++)
                    {
                        if (confusion_matrix_list[index].class_id != actual_class)
                        {
                            confusion_matrix_list[index].TN++;
                        }
                    }
                }

                else if (actual_class != predicted_class)
                {
                    actual_class_matrix.FN++;

                    predicted_class_matrix.FP++;
                }
            }

            foreach (var cm in confusion_matrix_list)
            {
                cm.calculate_metrics(calculate_auc, prediction_list);
                //if (calculate_auc)
                //{
                //    var (brier_score, roc_auc, roc_auc2, pr_auc, ap, api, roc_xy, pr_xy) = performance_measure.Calculate_ROC_PR_AUC(prediction_list, cm.class_id.Value);
                //    cm.Brier = brier_score;

                //    cm.ROC_AUC_Approx = roc_auc;
                //    cm.ROC_AUC2 = roc_auc2;

                //    cm.PR_AUC_Approx = pr_auc;
                //    cm.AP = ap;
                //    cm.API = api;

                //    cm.pr_xy_str = string.Join("/",pr_xy.Select(a => $"{Math.Round(a.x,6)};{Math.Round(a.y,6)}").ToList());
                //    cm.roc_xy_str = string.Join("/",roc_xy.Select(a => $"{Math.Round(a.x,6)};{Math.Round(a.y,6)}").ToList());
                //}


                //cm.TPR = (cm.TP + cm.FN) == (double) 0.0 ? (double) 0.0 : (double) (cm.TP) / (double) (cm.TP + cm.FN);
                //cm.TNR = (cm.TN + cm.FP) == (double) 0.0 ? (double) 0.0 : (double) (cm.TN) / (double) (cm.TN + cm.FP);
                //cm.PPV = (cm.TP + cm.FP) == (double) 0.0 ? (double) 0.0 : (double) (cm.TP) / (double) (cm.TP + cm.FP);
                //cm.NPV = (cm.TN + cm.FN) == (double) 0.0 ? (double) 0.0 : (double) (cm.TN) / (double) (cm.TN + cm.FN);
                //cm.FNR = (cm.FN + cm.TP) == (double) 0.0 ? (double) 0.0 : (double) (cm.FN) / (double) (cm.FN + cm.TP);
                //cm.FPR = (cm.FP + cm.TN) == (double) 0.0 ? (double) 0.0 : (double) (cm.FP) / (double) (cm.FP + cm.TN);
                //cm.FDR = (cm.FP + cm.TP) == (double) 0.0 ? (double) 0.0 : (double) (cm.FP) / (double) (cm.FP + cm.TP);
                //cm.FOR = (cm.FN + cm.TN) == (double) 0.0 ? (double) 0.0 : (double) (cm.FN) / (double) (cm.FN + cm.TN);
                //cm.ACC = (cm.P + cm.N) == (double) 0.0 ? (double) 0.0 : (double) (cm.TP + cm.TN) / (double) (cm.P + cm.N);
                //cm.F1S = (cm.PPV + cm.TPR) == (double) 0.0 ? (double) 0.0 : (double) (2 * cm.PPV * cm.TPR) / (double) (cm.PPV + cm.TPR);
                //cm.G1S = (cm.PPV + cm.TPR) == (double) 0.0 ? (double) 0.0 : (double) Math.Sqrt((double) (cm.PPV * cm.TPR));
                //cm.MCC = ((double) Math.Sqrt((double) ((cm.TP + cm.FP) * (cm.TP + cm.FN) * (cm.TN + cm.FP) * (cm.TN + cm.FN))) == (double) 0.0) ? (double) 0.0 : ((cm.TP * cm.TN) - (cm.FP * cm.FN)) / (double) Math.Sqrt((double) ((cm.TP + cm.FP) * (cm.TP + cm.FN) * (cm.TN + cm.FP) * (cm.TN + cm.FN)));
                //cm.BM_ = (cm.TPR + cm.TNR) - (double) 1.0;
                //cm.MK_ = (cm.PPV + cm.NPV) - (double) 1.0;
                //cm.BAC = (double) (cm.TPR + cm.TNR) / (double) 2.0;

                //cm.LRP = (1 - cm.TNR)== (double)0.0 ? (double)0.0 : (cm.TPR) / (1 - cm.TNR);
                //cm.LRN = (cm.TNR)==(double)0.0?(double)0.0:(1 - cm.TPR) / (cm.TNR);


                //cm.F1B_00 = fbeta2(cm.PPV, cm.TPR, (double) 0.0);
                //cm.F1B_01 = fbeta2(cm.PPV, cm.TPR, (double) 0.1);
                //cm.F1B_02 = fbeta2(cm.PPV, cm.TPR, (double) 0.2);
                //cm.F1B_03 = fbeta2(cm.PPV, cm.TPR, (double) 0.3);
                //cm.F1B_04 = fbeta2(cm.PPV, cm.TPR, (double) 0.4);
                //cm.F1B_05 = fbeta2(cm.PPV, cm.TPR, (double) 0.5);
                //cm.F1B_06 = fbeta2(cm.PPV, cm.TPR, (double) 0.6);
                //cm.F1B_07 = fbeta2(cm.PPV, cm.TPR, (double) 0.7);
                //cm.F1B_08 = fbeta2(cm.PPV, cm.TPR, (double) 0.8);
                //cm.F1B_09 = fbeta2(cm.PPV, cm.TPR, (double) 0.9);
                //cm.F1B_10 = fbeta2(cm.PPV, cm.TPR, (double) 1.0);

                //cm.calculate_ppf();
            }

            return confusion_matrix_list;
        }



        public static double area_under_curve_trapz(List<(double x, double y)> coordinate_list)//, bool interpolation = true)
        {
            
            //var param_list = new List<(string key, string value)>()
            //{
            //    (nameof(coordinate_list),coordinate_list.ToString()),
            //};

            //if (program.write_console_log) program.WriteLine($@"{nameof(area_under_curve_trapz)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            

            //var coords = new List<(double x1, double x2, double y1, double y2)>();
            coordinate_list = coordinate_list.Distinct().ToList();
            coordinate_list = coordinate_list.OrderBy(a => a.x).ThenBy(a => a.y).ToList();
            var auc = coordinate_list.Select((c, i) => i >= coordinate_list.Count - 1 ? 0 : (coordinate_list[i + 1].x - coordinate_list[i].x) * ((coordinate_list[i].y + coordinate_list[i + 1].y) / 2)).Sum();
            return auc;
        }

        public static List<prediction> load_prediction_file_regression_values(string test_file, string test_comments_file, string prediction_file)
        {
            if (string.IsNullOrWhiteSpace(test_file) || !File.Exists(test_file) || new FileInfo(test_file).Length == 0)
            {
                throw new Exception($@"Error: Test data file not found: ""{test_file}"".");
            }

            if (string.IsNullOrWhiteSpace(prediction_file) || !File.Exists(prediction_file) || new FileInfo(prediction_file).Length == 0)
            {
                throw new Exception($@"Error: Prediction output file not found: ""{prediction_file}"".");
            }


          

            var test_file_lines = File.ReadAllLines(test_file).ToList();

            test_file_lines = test_file_lines.Select(a =>
            {
                var hash_index = a.IndexOf('#');

                if (hash_index > -1)
                {
                    return a.Substring(0, hash_index).Trim();
                }

                return a.Trim();
            }).ToList();

            var test_file_comments = !string.IsNullOrWhiteSpace(test_comments_file) ? File.ReadAllLines(test_comments_file).ToList() : null;

            //var test_file_comments = test_file_lines.Where(a => !string.IsNullOrWhiteSpace(a) && int.TryParse(a.Split().First(), out var line_cid)).Select(a =>
            //{
            //    var hash_index = a.IndexOf('#');
            //    if (a.Length > hash_index + 1) return a.Substring(hash_index + 1);
            //    else return "";
            //}).ToList();


            var test_file_data = test_file_lines.Where(a => !string.IsNullOrWhiteSpace(a) && int.TryParse(a.Split().First(), out var line_cid)).Select(a => a.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToList();

            //var test_comments_file_data = test_comments_file_lines.Where(a => !string.IsNullOrWhiteSpace(a) && int.TryParse(a.Split().First(), out var line_cid)).Select(a => a.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToList();

            if (test_file_lines.Count == 0) return null;

            var prediction_file_lines = File.ReadAllLines(prediction_file);
            var prediction_file_data = prediction_file_lines.Where(a => !string.IsNullOrWhiteSpace(a) && int.TryParse(a.Split().First(), out var line_cid)).Select(a => a.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            if (prediction_file_lines.Length == 0) return null;

            var probability_estimate_class_labels = new List<int>();
            if (prediction_file_lines.First().Trim().Split().First() == "labels")
            {
                probability_estimate_class_labels = prediction_file_lines.First().Trim().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(int.Parse).ToList();
            }

            if (test_file_comments != null && test_file_data.Count != test_file_comments.Count)
            {
                throw new Exception("Error: test file and test comments file have different instance length: " + test_file_data.Count + " " + test_file_comments.Count);
            }

            if (test_file_data.Count != prediction_file_data.Count)
            {
                throw new Exception("Error: test file and prediction file have different instance length: " + test_file_data.Count + " " + prediction_file_data.Count);
            }


            var prediction_list = new List<prediction>();

            for (var prediction_index = 0; prediction_index < test_file_data.Count; prediction_index++)
            {
                var probability_estimates = (prediction_file_data[prediction_index].Length <= 1) ?
                    new List<(int, double)>() :
                    prediction_file_data[prediction_index].Skip(1).Select((a, i) => (class_id: probability_estimate_class_labels[i], probability_estimate: double.Parse(a, NumberStyles.Float, CultureInfo.InvariantCulture))).ToList();

                var probability_estimates_stated = probability_estimates != null && probability_estimates.Count > 0 && probability_estimate_class_labels != null && probability_estimate_class_labels.Count > 0;

                var prediction = new prediction()
                {
                    comment = test_file_comments != null ? test_file_comments[prediction_index] : "",
                    prediction_index = prediction_index,
                    actual_class = int.Parse(test_file_data[prediction_index][0], CultureInfo.InvariantCulture),
                    default_predicted_class = int.Parse(prediction_file_data[prediction_index][0], CultureInfo.InvariantCulture),
                    probability_estimates_stated = probability_estimates_stated,
                    probability_estimates = probability_estimates,
                    test_row_vector = test_file_data[prediction_index].Skip(1).ToArray(),
                };

                prediction_list.Add(prediction);
            }

            return prediction_list;
        }

        public static List<confusion_matrix> load_prediction_file(string test_file, string test_comments_file, string prediction_file, bool output_threshold_adjustment_performance)
        {
            
            //var param_list = new List<(string key, string value)>()
            //{
            //    (nameof(test_file),test_file.ToString()),
            //    (nameof(prediction_file),prediction_file.ToString()),
            //    (nameof(output_threshold_adjustment_performance),output_threshold_adjustment_performance.ToString()),
            //};

            //if (program.write_console_log) program.WriteLine($@"{nameof(load_prediction_file)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            

            var prediction_list = load_prediction_file_regression_values(test_file, test_comments_file, prediction_file);
            var cms = load_prediction_file(prediction_list, output_threshold_adjustment_performance);

            return cms;
        }

        public static List<confusion_matrix> load_prediction_file(List<prediction> prediction_list, bool output_threshold_adjustment_performance)
        {
            
            //var param_list = new List<(string key, string value)>()
            //{
            //    (nameof(prediction_list),prediction_list.ToString()),
            //    (nameof(output_threshold_adjustment_performance),output_threshold_adjustment_performance.ToString()),
            //};

            //if (program.write_console_log) program.WriteLine($@"{nameof(load_prediction_file)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            

            var class_id_list = prediction_list.Select(a => a.actual_class).Distinct().OrderBy(a => a).ToList();

            var default_confusion_matrix_list = count_prediction_error(prediction_list);

            var confusion_matrix_list = new List<confusion_matrix>();
            confusion_matrix_list.AddRange(default_confusion_matrix_list);

            if (class_id_list.Count >= 2 && output_threshold_adjustment_performance)
            {
                var positive_id = class_id_list.Max();
                var negative_id = class_id_list.Min();

                var thresholds = new List<double>();
                for (var i = 0.00m; i <= 1.00m; i += 0.05m)
                {
                    thresholds.Add((double)i);
                }



                var threshold_prediction_list = thresholds.Select(t => (positive_threshold: t, prediction_list: prediction_list.Select(p => new prediction()
                {
                    actual_class = p.actual_class,
                    default_predicted_class = p.probability_estimates.First(e => e.class_id == positive_id).probability_estimate >= t ? positive_id : negative_id,
                    probability_estimates = p.probability_estimates,
                    prediction_index = p.prediction_index,
                    test_row_vector = p.test_row_vector,
                    comment = p.comment,
                }).ToList())).ToList();

                var threshold_confusion_matrix_list = threshold_prediction_list.SelectMany(a => count_prediction_error(a.prediction_list, a.positive_threshold, positive_id, false)).ToList();

                for (var i = 0; i < threshold_confusion_matrix_list.Count; i++)
                {
                    var default_cm = default_confusion_matrix_list.First(a => a.class_id == threshold_confusion_matrix_list[i].class_id);

                    threshold_confusion_matrix_list[i].ROC_AUC_Approx_All = default_cm.ROC_AUC_Approx_All;
                    threshold_confusion_matrix_list[i].ROC_AUC_Approx_11p = default_cm.ROC_AUC_Approx_11p;
                    threshold_confusion_matrix_list[i].roc_xy_str_all = default_cm.roc_xy_str_all;
                    threshold_confusion_matrix_list[i].roc_xy_str_11p = default_cm.roc_xy_str_11p;

                    threshold_confusion_matrix_list[i].PR_AUC_Approx_All = default_cm.PR_AUC_Approx_All;
                    threshold_confusion_matrix_list[i].PR_AUC_Approx_11p = default_cm.PR_AUC_Approx_11p;
                    threshold_confusion_matrix_list[i].pr_xy_str_all = default_cm.pr_xy_str_all;
                    threshold_confusion_matrix_list[i].pr_xy_str_11p = default_cm.pr_xy_str_11p;
                    threshold_confusion_matrix_list[i].pri_xy_str_all = default_cm.pri_xy_str_all;
                    threshold_confusion_matrix_list[i].pri_xy_str_11p = default_cm.pri_xy_str_11p;
                }

                confusion_matrix_list.AddRange(threshold_confusion_matrix_list);
            }

            return confusion_matrix_list;
        }

        public static double brier(List<prediction> prediction_list, int positive_id)
        {
            if (prediction_list.Any(a => !a.probability_estimates_stated)) return default;

            prediction_list = prediction_list.OrderByDescending(a => a.probability_estimates.First(b => b.class_id == positive_id).probability_estimate).ToList();

            // Calc Brier
            var brier_score = ((double)1 / (double)prediction_list.Count)
                              * (prediction_list.Sum(a => Math.Pow(a.probability_estimates.First(b => b.class_id == a.default_predicted_class).probability_estimate - (a.actual_class == a.default_predicted_class ? 1 : 0), 2)));

            return brier_score;
        }

        public enum threshold_type
        {
            all_thresholds,
            eleven_points
        }

        public static (/*double brier_score,*/ double roc_auc_approx, double roc_auc_actual, double pr_auc_approx, double pri_auc_approx, double ap, double api, List<(double x, double y)> roc_xy, List<(double x, double y)> pr_xy, List<(double x, double y)> pri_xy)
            Calculate_ROC_PR_AUC(List<prediction> prediction_list, int positive_id, threshold_type threshold_type = threshold_type.all_thresholds)
        {
            if (prediction_list.Any(a => !a.probability_estimates_stated)) return default;

            // Assume binary classifier - get negative class id
            var negative_id = prediction_list.First(a => a.actual_class != positive_id).actual_class;

            // Calc P
            var p = prediction_list.Count(a => a.actual_class == positive_id);

            // Calc N
            var n = prediction_list.Count(a => a.actual_class == negative_id);

            // Order predictions descending by positive class probability
            prediction_list = prediction_list.OrderByDescending(a => a.probability_estimates.FirstOrDefault(b => b.class_id == positive_id).probability_estimate).ToList();

            // Get thresholds list (either all thresholds or 11 points)
            List<double> thresholds = null;

            if (threshold_type == threshold_type.all_thresholds)
            {
                thresholds = prediction_list.Select(a => a.probability_estimates.FirstOrDefault(b => b.class_id == positive_id).probability_estimate).Distinct().OrderByDescending(a => a).ToList();
            }
            else if (threshold_type == threshold_type.eleven_points)
            {
                thresholds = new List<double>()
                {
                    1.0, 0.9, 0.8, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1, 0.0
                };
            }
            else
            {
                throw new NotSupportedException();
            }

            // Calc predictions at each threshold
            var threshold_prediction_list = thresholds.Select(t => (positive_threshold: t, prediction_list: prediction_list.Select(pl => new prediction()
            {
                comment = pl.comment,
                actual_class = pl.actual_class,
                default_predicted_class = pl.probability_estimates.FirstOrDefault(e => e.class_id == positive_id).probability_estimate >= t ? positive_id : negative_id,
                probability_estimates = pl.probability_estimates,
                prediction_index = pl.prediction_index,
                test_row_vector = pl.test_row_vector
            }).ToList())).ToList();

            // Calc confusion matrices at each threshold
            var threshold_confusion_matrix_list = threshold_prediction_list.SelectMany(a => count_prediction_error(a.prediction_list, a.positive_threshold, positive_id, false)).ToList();
            threshold_confusion_matrix_list = threshold_confusion_matrix_list.Where(a => a.class_id == positive_id).ToList();

            //// Calc Brier
            //var brier_score = ((double)1 / (double)prediction_list.Count)
            //                  * (prediction_list.Sum(a => Math.Pow(a.probability_estimates.First(b => b.class_id == a.default_predicted_class).probability_estimate - (a.actual_class == a.default_predicted_class ? 1 : 0), 2)));

            // Average Precision (Not Approximated)
            var ap = threshold_confusion_matrix_list.Select((a, i) =>
            {
                //var max_p = threshold_confusion_matrix_list.Where(b => b.TPR >= a.TPR).Max(b => b.PPV);
                var delta_tpr = Math.Abs(a.TPR - (i == 0 ? 0 : threshold_confusion_matrix_list[i - 1].TPR));
                var _ap = a.PPV * delta_tpr;

                if (double.IsNaN(_ap)) _ap = 0;
                //var _api = max_p * delta_tpr;
                return _ap;
            }).Sum();

            // Average Precision Interpolated (Not Approximated)
            var api = threshold_confusion_matrix_list.Select((a, i) =>
            {
                var max_ppv = threshold_confusion_matrix_list.Where(b => b.TPR >= a.TPR).Max(b => b.PPV);

                if (double.IsNaN(max_ppv)/* || max_ppv == 0*/) max_ppv = a.PPV; // = 0? =1? unknown: should it be a.PPV, 0, or 1 when there are no true results?

                 var delta_tpr = Math.Abs(a.TPR - (i == 0 ? 0 : threshold_confusion_matrix_list[i - 1].TPR));
                //var _ap = a.PPV * delta_tpr;
                var _api = max_ppv * delta_tpr;

                if (double.IsNaN(_api)) _api = 0;

                return _api;
            }).Sum();

            // PR Curve Coordinates
            var pr_plot_coords = threshold_confusion_matrix_list.Select(a => (x: a.TPR, y: a.PPV)).ToList();

            if (pr_plot_coords.First().x != 0.0)
            {
                pr_plot_coords.Insert(0, ((double)0.0, pr_plot_coords.First().y));
            }

            if (pr_plot_coords.Last().x != 1.0 && threshold_confusion_matrix_list.Count > 0)
            {
                var m = threshold_confusion_matrix_list.First();
                pr_plot_coords.Add(((double)1.0, (double)m.P / ((double)m.P + (double)m.N)));
            }

            // PRI Curve Coordinates
            var pri_plot_coords = threshold_confusion_matrix_list.Select(a =>
            {
                var max_ppv = threshold_confusion_matrix_list.Where(b => b.TPR >= a.TPR).Max(b => b.PPV);
                if (double.IsNaN(max_ppv)) max_ppv = a.PPV;// 0;

                return (x: a.TPR, y: max_ppv);
            }).ToList();

            if (pri_plot_coords.First().x != 0.0)
            {
                pri_plot_coords.Insert(0, ((double)0.0, pri_plot_coords.First().y));
            }

            if (pri_plot_coords.Last().x != 1.0 && threshold_confusion_matrix_list.Count > 0)
            {
                var m = threshold_confusion_matrix_list.First();
                pri_plot_coords.Add(((double)1.0, (double)m.P / ((double)m.P + (double)m.N)));
            }

            // ROC Curve Coordinates
            var roc_plot_coords = threshold_confusion_matrix_list.Select(a => (x: a.FPR, y: a.TPR)).ToList();
            if (!roc_plot_coords.Any(a => a.x == (double)0.0 && a.y == (double)0.0)) roc_plot_coords.Insert(0, ((double)0.0, (double)0.0));
            if (!roc_plot_coords.Any(a => a.x == (double)1.0 && a.y == (double)1.0)) roc_plot_coords.Add(((double)1.0, (double)1.0));
            roc_plot_coords = roc_plot_coords.Distinct().ToList();

            // ROC Approx
            var roc_auc_approx = area_under_curve_trapz(roc_plot_coords);
            roc_plot_coords = roc_plot_coords.OrderBy(a => a.y).ThenBy(a => a.x).ToList();

            // PR Approx
            var pr_auc_approx = area_under_curve_trapz(pr_plot_coords);

            // PRI Approx
            var pri_auc_approx = area_under_curve_trapz(pri_plot_coords);

            // ROC (Not Approx & Not Eleven Point - Incompatible)
            
            var total_neg_for_threshold = prediction_list.Select((a, i) => (actual_class: a.actual_class, total_neg_at_point: prediction_list.Where((b, j) => j <= i && b.actual_class == negative_id).Count())).ToList();
            var roc_auc_actual = ((double)1 / (double)(p * n)) * (double)prediction_list
                                  .Select((a, i) =>
                                  {
                                      if (a.actual_class != positive_id) return 0;
                                      var total_n_at_current_threshold = total_neg_for_threshold[i].total_neg_at_point;

                                      var n_more_than_current_n = total_neg_for_threshold.Count(b => b.actual_class == negative_id && b.total_neg_at_point > total_n_at_current_threshold);

                                      return n_more_than_current_n;
                                  }).Sum();
           
            return (/*brier_score: brier_score,*/ roc_auc_approx: roc_auc_approx, roc_auc_actual: roc_auc_actual, pr_auc_approx: pr_auc_approx, pri_auc_approx: pri_auc_approx, ap: ap, api: api, roc_xy: roc_plot_coords, pr_xy: pr_plot_coords, pri_xy: pri_plot_coords);
        }
    }
}
