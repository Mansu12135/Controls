﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Transactions;
using Microsoft.Win32.SafeHandles;

namespace ApplicationLayer
{
    internal sealed class TransactionFile
    {
        [DllImport("Kernel32.Dll", EntryPoint = "CreateFileTransacted", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern SafeFileHandle CreateFileTransacted(
            [In] String lpFileName,
            [In] SafeTransactionHandle.FileAccess dwDesiredAccess,
            [In] SafeTransactionHandle.FileShare dwShareMode,
            [In] IntPtr lpSecurityAttributes,
            [In] SafeTransactionHandle.FileMode dwCreationDisposition,
            [In] int dwFlagsAndAttributes,
            [In] IntPtr hTemplateFile,
            [In] SafeTransactionHandle hTransaction,
            [In] IntPtr pusMiniVersion,
            [In] IntPtr pExtendedParameter
        );

        [DllImport("Kernel32.Dll", EntryPoint = "CopyFileTransacted", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern bool CopyFileTransacted(
           [In] String lpExistingFileName,
           [In] String lpNewFileName,
           [In] IntPtr lpProgressRoutine,
           [In] IntPtr lpData,
           [In] bool pbCancel,
           [In] SafeTransactionHandle.Copy dwCopyFlags,
           [In] SafeTransactionHandle hTransaction
       );

        [DllImport("Kernel32.Dll", EntryPoint = "OpenFile", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern SafeFileHandle OpenFile(
          [In] String lpFileName,
          [In] _OFSTRUCT lpReOpenBuff,
          [In] IntPtr uStyle
      );

        [DllImport("Kernel32.Dll", EntryPoint = "MoveFileTransacted", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern bool MoveFileTransacted(
           [In] String lpExistingFileName,
           [In] String lpNewFileName,
           [In] IntPtr lpProgressRoutine,
           [In] IntPtr lpData,
           [In] SafeTransactionHandle.Move dwFlags,
           [In] SafeTransactionHandle hTransaction
       );

        [DllImport("Kernel32.Dll", EntryPoint = "DeleteFileTransacted", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern bool DeleteFileTransacted(
            [In] String lpFileName,
            [In] SafeTransactionHandle hTransaction
        );

        [ComImport]
        [Guid("79427A2B-F895-40e0-BE79-B57DC82ED231")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        protected interface IKernelTransaction
        {
            void GetHandle(out SafeTransactionHandle ktmHandle);
        }

        private static SafeFileHandle CreateFileHandled(string path, ref string message)
        {
           
            SafeTransactionHandle txHandle = null;
            SafeFileHandle fileHandle = null;
            try
            {
                IKernelTransaction kernelTx =
                    (IKernelTransaction)TransactionInterop.GetDtcTransaction(Transaction.Current);
                kernelTx.GetHandle(out txHandle);

                fileHandle
                    = CreateFileTransacted(
                        path
                        , SafeTransactionHandle.FileAccess.GENERIC_WRITE
                        , SafeTransactionHandle.FileShare.FILE_SHARE_NONE
                        , IntPtr.Zero
                        , SafeTransactionHandle.FileMode.CREATE_ALWAYS
                        , 0
                        , IntPtr.Zero
                        , txHandle
                        , IntPtr.Zero
                        , IntPtr.Zero);
                if (Path.GetExtension(path) == ".rtf")
                {
                    WriteToFile(Encoding.ASCII.GetBytes(@"{\rtf1\adeflang1025\ansi\ansicpg1251\uc1\adeff31507\deff0\stshfdbch31506\stshfloch31506\stshfhich31506\stshfbi31507\deflang1058\deflangfe1058\themelang1058\themelangfe0\themelangcs0{\fonttbl{\f0\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304}Times New Roman;}
{\f0\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304} Times New Roman; }
                    {\f37\fbidi \fswiss\fcharset204\fprq2{\*\panose 020f0502020204030204} Calibri; }
                    {\flomajor\f31500\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304} Times New Roman; }
                    {\fdbmajor\f31501\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304} Times New Roman; }
                    {\fhimajor\f31502\fbidi \froman\fcharset204\fprq2{\*\panose 02040503050406030204} Cambria; }
                    {\fbimajor\f31503\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304} Times New Roman; }
                    {\flominor\f31504\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304} Times New Roman; }
                    {\fdbminor\f31505\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304} Times New Roman; }
                    {\fhiminor\f31506\fbidi \fswiss\fcharset204\fprq2{\*\panose 020f0502020204030204} Calibri; }
                    {\fbiminor\f31507\fbidi \froman\fcharset204\fprq2{\*\panose 02020603050405020304} Times New Roman; }
                    {\f41\fbidi \froman\fcharset0\fprq2 Times New Roman; }
                    {\f39\fbidi \froman\fcharset238\fprq2 Times New Roman CE; }
                    {\f42\fbidi \froman\fcharset161\fprq2 Times New Roman Greek; }
                    {\f43\fbidi \froman\fcharset162\fprq2 Times New Roman Tur; }
                    {\f44\fbidi \froman\fcharset177\fprq2 Times New Roman(Hebrew); }
                    {\f45\fbidi \froman\fcharset178\fprq2 Times New Roman(Arabic); }
                    {\f46\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic; }
                    {\f47\fbidi \froman\fcharset163\fprq2 Times New Roman(Vietnamese); }
                    {\f41\fbidi \froman\fcharset0\fprq2 Times New Roman; }
                    {\f39\fbidi \froman\fcharset238\fprq2 Times New Roman CE; }
                    {\f42\fbidi \froman\fcharset161\fprq2 Times New Roman Greek; }
                    {\f43\fbidi \froman\fcharset162\fprq2 Times New Roman Tur; }
                    {\f44\fbidi \froman\fcharset177\fprq2 Times New Roman(Hebrew); }
                    {\f45\fbidi \froman\fcharset178\fprq2 Times New Roman(Arabic); }
                    {\f46\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic; }
                    {\f47\fbidi \froman\fcharset163\fprq2 Times New Roman(Vietnamese); }
                    {\f411\fbidi \fswiss\fcharset0\fprq2 Calibri; }
                    {\f409\fbidi \fswiss\fcharset238\fprq2 Calibri CE; }
                    {\f412\fbidi \fswiss\fcharset161\fprq2 Calibri Greek; }
                    {\f413\fbidi \fswiss\fcharset162\fprq2 Calibri Tur; }
                    {\f416\fbidi \fswiss\fcharset186\fprq2 Calibri Baltic; }
                    {\f417\fbidi \fswiss\fcharset163\fprq2 Calibri(Vietnamese); }
                    {\flomajor\f31510\fbidi \froman\fcharset0\fprq2 Times New Roman; }
                    {\flomajor\f31508\fbidi \froman\fcharset238\fprq2 Times New Roman CE; }
                    {\flomajor\f31511\fbidi \froman\fcharset161\fprq2 Times New Roman Greek;}{\flomajor\f31512\fbidi \froman\fcharset162\fprq2 Times New Roman Tur;}{\flomajor\f31513\fbidi \froman\fcharset177\fprq2 Times New Roman (Hebrew);}
{\flomajor\f31514\fbidi \froman\fcharset178\fprq2 Times New Roman (Arabic);}{\flomajor\f31515\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic;}{\flomajor\f31516\fbidi \froman\fcharset163\fprq2 Times New Roman (Vietnamese);}
{\fdbmajor\f31520\fbidi \froman\fcharset0\fprq2 Times New Roman;}{\fdbmajor\f31518\fbidi \froman\fcharset238\fprq2 Times New Roman CE;}{\fdbmajor\f31521\fbidi \froman\fcharset161\fprq2 Times New Roman Greek;}
{\fdbmajor\f31522\fbidi \froman\fcharset162\fprq2 Times New Roman Tur;}{\fdbmajor\f31523\fbidi \froman\fcharset177\fprq2 Times New Roman (Hebrew);}{\fdbmajor\f31524\fbidi \froman\fcharset178\fprq2 Times New Roman (Arabic);}
{\fdbmajor\f31525\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic;}{\fdbmajor\f31526\fbidi \froman\fcharset163\fprq2 Times New Roman (Vietnamese);}{\fhimajor\f31530\fbidi \froman\fcharset0\fprq2 Cambria;}
{\fhimajor\f31528\fbidi \froman\fcharset238\fprq2 Cambria CE;}{\fhimajor\f31531\fbidi \froman\fcharset161\fprq2 Cambria Greek;}{\fhimajor\f31532\fbidi \froman\fcharset162\fprq2 Cambria Tur;}
{\fhimajor\f31535\fbidi \froman\fcharset186\fprq2 Cambria Baltic;}{\fhimajor\f31536\fbidi \froman\fcharset163\fprq2 Cambria (Vietnamese);}{\fbimajor\f31540\fbidi \froman\fcharset0\fprq2 Times New Roman;}
{\fbimajor\f31538\fbidi \froman\fcharset238\fprq2 Times New Roman CE;}{\fbimajor\f31541\fbidi \froman\fcharset161\fprq2 Times New Roman Greek;}{\fbimajor\f31542\fbidi \froman\fcharset162\fprq2 Times New Roman Tur;}
{\fbimajor\f31543\fbidi \froman\fcharset177\fprq2 Times New Roman (Hebrew);}{\fbimajor\f31544\fbidi \froman\fcharset178\fprq2 Times New Roman (Arabic);}{\fbimajor\f31545\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic;}
{\fbimajor\f31546\fbidi \froman\fcharset163\fprq2 Times New Roman (Vietnamese);}{\flominor\f31550\fbidi \froman\fcharset0\fprq2 Times New Roman;}{\flominor\f31548\fbidi \froman\fcharset238\fprq2 Times New Roman CE;}
{\flominor\f31551\fbidi \froman\fcharset161\fprq2 Times New Roman Greek;}{\flominor\f31552\fbidi \froman\fcharset162\fprq2 Times New Roman Tur;}{\flominor\f31553\fbidi \froman\fcharset177\fprq2 Times New Roman (Hebrew);}
{\flominor\f31554\fbidi \froman\fcharset178\fprq2 Times New Roman (Arabic);}{\flominor\f31555\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic;}{\flominor\f31556\fbidi \froman\fcharset163\fprq2 Times New Roman (Vietnamese);}
{\fdbminor\f31560\fbidi \froman\fcharset0\fprq2 Times New Roman;}{\fdbminor\f31558\fbidi \froman\fcharset238\fprq2 Times New Roman CE;}{\fdbminor\f31561\fbidi \froman\fcharset161\fprq2 Times New Roman Greek;}
{\fdbminor\f31562\fbidi \froman\fcharset162\fprq2 Times New Roman Tur;}{\fdbminor\f31563\fbidi \froman\fcharset177\fprq2 Times New Roman (Hebrew);}{\fdbminor\f31564\fbidi \froman\fcharset178\fprq2 Times New Roman (Arabic);}
{\fdbminor\f31565\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic;}{\fdbminor\f31566\fbidi \froman\fcharset163\fprq2 Times New Roman (Vietnamese);}{\fhiminor\f31570\fbidi \fswiss\fcharset0\fprq2 Calibri;}
{\fhiminor\f31568\fbidi \fswiss\fcharset238\fprq2 Calibri CE;}{\fhiminor\f31571\fbidi \fswiss\fcharset161\fprq2 Calibri Greek;}{\fhiminor\f31572\fbidi \fswiss\fcharset162\fprq2 Calibri Tur;}
{\fhiminor\f31575\fbidi \fswiss\fcharset186\fprq2 Calibri Baltic;}{\fhiminor\f31576\fbidi \fswiss\fcharset163\fprq2 Calibri (Vietnamese);}{\fbiminor\f31580\fbidi \froman\fcharset0\fprq2 Times New Roman;}
{\fbiminor\f31578\fbidi \froman\fcharset238\fprq2 Times New Roman CE;}{\fbiminor\f31581\fbidi \froman\fcharset161\fprq2 Times New Roman Greek;}{\fbiminor\f31582\fbidi \froman\fcharset162\fprq2 Times New Roman Tur;}
{\fbiminor\f31583\fbidi \froman\fcharset177\fprq2 Times New Roman (Hebrew);}{\fbiminor\f31584\fbidi \froman\fcharset178\fprq2 Times New Roman (Arabic);}{\fbiminor\f31585\fbidi \froman\fcharset186\fprq2 Times New Roman Baltic;}
{\fbiminor\f31586\fbidi \froman\fcharset163\fprq2 Times New Roman (Vietnamese);}}{\colortbl;\red0\green0\blue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red255\green255\blue0;
\red255\green255\blue255;\red0\green0\blue128;\red0\green128\blue128;\red0\green128\blue0;\red128\green0\blue128;\red128\green0\blue0;\red128\green128\blue0;\red128\green128\blue128;\red192\green192\blue192;}{\*\defchp 
\f31506\fs22\lang1058\langfe1033\langfenp1033 }{\*\defpap \ql \li0\ri0\sa200\sl276\slmult1\widctlpar\wrapdefault\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0 }\noqfpromote {\stylesheet{\ql \li0\ri0\sa200\sl276\slmult1
\widctlpar\wrapdefault\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0 \rtlch\fcs1 \af31507\afs22\alang1025 \ltrch\fcs0 \f31506\fs22\lang1058\langfe1033\cgrid\langnp1058\langfenp1033 \snext0 \sqformat \spriority0 Normal;}{\*\cs10 \additive 
\ssemihidden \sunhideused \spriority1 Default Paragraph Font;}{\*
\ts11\tsrowd\trftsWidthB3\trpaddl108\trpaddr108\trpaddfl3\trpaddft3\trpaddfb3\trpaddfr3\trcbpat1\trcfpat1\tblind0\tblindtype3\tsvertalt\tsbrdrt\tsbrdrl\tsbrdrb\tsbrdrr\tsbrdrdgl\tsbrdrdgr\tsbrdrh\tsbrdrv \ql \li0\ri0\sa200\sl276\slmult1
\widctlpar\wrapdefault\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0 \rtlch\fcs1 \af31507\afs22\alang1025 \ltrch\fcs0 \f31506\fs22\lang1058\langfe1033\cgrid\langnp1058\langfenp1033 \snext11 \ssemihidden \sunhideused Normal Table;}}
{\*\rsidtbl \rsid5076400\rsid13008422}{\mmathPr\mmathFont34\mbrkBin0\mbrkBinSub0\msmallFrac0\mdispDef1\mlMargin0\mrMargin0\mdefJc1\mwrapIndent1440\mintLim0\mnaryLim1}{\info{\author \'cd\'e8\'ea\'e8\'f2\'e0}{\operator \'cd\'e8\'ea\'e8\'f2\'e0}
{\creatim\yr2017\mo4\dy8\hr22\min14}{\revtim\yr2017\mo4\dy8\hr22\min14}{\version1}{\edmins0}{\nofpages1}{\nofwords0}{\nofchars0}{\nofcharsws0}{\vern49167}}{\*\xmlnstbl {\xmlns1 http://schemas.microsoft.com/office/word/2003/wordml}}
\paperw11906\paperh16838\margl1417\margr850\margt850\margb850\gutter0\ltrsect 
\deftab708\widowctrl\ftnbj\aenddoc\hyphhotz425\trackmoves0\trackformatting1\donotembedsysfont1\relyonvml0\donotembedlingdata0\grfdocevents0\validatexml1\showplaceholdtext0\ignoremixedcontent0\saveinvalidxml0
\showxmlerrors1\noxlattoyen\expshrtn\noultrlspc\dntblnsbdb\nospaceforul\formshade\horzdoc\dgmargin\dghspace180\dgvspace180\dghorigin1417\dgvorigin850\dghshow1\dgvshow1
\jexpand\viewkind1\viewscale100\pgbrdrhead\pgbrdrfoot\splytwnine\ftnlytwnine\htmautsp\nolnhtadjtbl\useltbaln\alntblind\lytcalctblwd\lyttblrtgr\lnbrkrule\nobrkwrptbl\snaptogridincell\allowfieldendsel\wrppunct
\asianbrkrule\rsidroot5076400\newtblstyruls\nogrowautofit\usenormstyforlist\noindnmbrts\felnbrelev\nocxsptable\indrlsweleven\noafcnsttbl\afelev\utinl\hwelev\spltpgpar\notcvasp\notbrkcnstfrctbl\notvatxbx\krnprsnet\cachedcolbal \nouicompat \fet0
{\*\wgrffmtfilter 2450}\nofeaturethrottle1\ilfomacatclnup0\ltrpar \sectd \ltrsect\linex0\headery708\footery708\colsx708\endnhere\sectlinegrid360\sectdefaultcl\sftnbj {\*\pnseclvl1\pnucrm\pnstart1\pnindent720\pnhang {\pntxta .}}{\*\pnseclvl2
\pnucltr\pnstart1\pnindent720\pnhang {\pntxta .}}{\*\pnseclvl3\pndec\pnstart1\pnindent720\pnhang {\pntxta .}}{\*\pnseclvl4\pnlcltr\pnstart1\pnindent720\pnhang {\pntxta )}}{\*\pnseclvl5\pndec\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}{\*\pnseclvl6
\pnlcltr\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}{\*\pnseclvl7\pnlcrm\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}{\*\pnseclvl8\pnlcltr\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}{\*\pnseclvl9\pnlcrm\pnstart1\pnindent720\pnhang 
{\pntxtb (}{\pntxta )}}\pard\plain \ltrpar\ql \li0\ri0\sa200\sl276\slmult1\widctlpar\wrapdefault\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0 \rtlch\fcs1 \af31507\afs22\alang1025 \ltrch\fcs0 
\f31506\fs22\lang1058\langfe1033\cgrid\langnp1058\langfenp1033 {\rtlch\fcs1 \af31507 \ltrch\fcs0 \insrsid13008422 
\par }{\*\themedata 504b030414000600080000002100e9de0fbfff0000001c020000130000005b436f6e74656e745f54797065735d2e786d6cac91cb4ec3301045f748fc83e52d4a
9cb2400825e982c78ec7a27cc0c8992416c9d8b2a755fbf74cd25442a820166c2cd933f79e3be372bd1f07b5c3989ca74aaff2422b24eb1b475da5df374fd9ad
5689811a183c61a50f98f4babebc2837878049899a52a57be670674cb23d8e90721f90a4d2fa3802cb35762680fd800ecd7551dc18eb899138e3c943d7e503b6
b01d583deee5f99824e290b4ba3f364eac4a430883b3c092d4eca8f946c916422ecab927f52ea42b89a1cd59c254f919b0e85e6535d135a8de20f20b8c12c3b0
0c895fcf6720192de6bf3b9e89ecdbd6596cbcdd8eb28e7c365ecc4ec1ff1460f53fe813d3cc7f5b7f020000ffff0300504b030414000600080000002100a5d6
a7e7c0000000360100000b0000005f72656c732f2e72656c73848fcf6ac3300c87ef85bd83d17d51d2c31825762fa590432fa37d00e1287f68221bdb1bebdb4f
c7060abb0884a4eff7a93dfeae8bf9e194e720169aaa06c3e2433fcb68e1763dbf7f82c985a4a725085b787086a37bdbb55fbc50d1a33ccd311ba548b6309512
0f88d94fbc52ae4264d1c910d24a45db3462247fa791715fd71f989e19e0364cd3f51652d73760ae8fa8c9ffb3c330cc9e4fc17faf2ce545046e37944c69e462
a1a82fe353bd90a865aad41ed0b5b8f9d6fd010000ffff0300504b0304140006000800000021006b799616830000008a0000001c0000007468656d652f746865
6d652f7468656d654d616e616765722e786d6c0ccc4d0ac3201040e17da17790d93763bb284562b2cbaebbf600439c1a41c7a0d29fdbd7e5e38337cedf14d59b
4b0d592c9c070d8a65cd2e88b7f07c2ca71ba8da481cc52c6ce1c715e6e97818c9b48d13df49c873517d23d59085adb5dd20d6b52bd521ef2cdd5eb9246a3d8b
4757e8d3f729e245eb2b260a0238fd010000ffff0300504b030414000600080000002100a55e7d2dc7060000d71b0000160000007468656d652f7468656d652f
7468656d65312e786d6cec59cf6e1b4518bf23f10ea3bdb7b113278da33a55ecd80db469a3d82dea71bc1eef4e33bbb39a1927f5ad4a8f482044411ca8045c38
2020528bb8b4efe03e43a0088ad457e09b99ddf54ebca1491b4105cd21de9dfd7dffffcc37bb172fdd8918da2542521e37bceaf98a8748ecf3018d838677a3d7
39b7ec21a9703cc08cc7a4e18d89f42eadbefbce45bca242121104f4b15cc10d2f542a59999b933e2c63799e27248667432e22ace05604730381f7806fc4e6e6
2b95a5b908d3d843318e80ede49bc94f93c79303747d38a43ef15633fe6d06426225f582cf4457732719d1d74ff72707932793479383a777e1fa09fc7e6c6807
3b554d21c7b2c504dac5ace181e801dfeb913bca430c4b050f1a5ec5fc7973ab17e7f04a4ac4d431b405ba8ef94be95282c1cebc9129827e2eb4daa9d52face7
fc0d80a9595cbbdd6eb5ab393f03c0be0f965b5d8a3c6b9de56a33e35900d9cb59deadca62a5e6e20bfc176674ae379bcdc57aaa8b656a40f6b236835fae2cd5
d6e61dbc0159fce20cbed65c6bb5961cbc0159fcd20cbe73a1be5473f10614321aefcca075403b9d947b0e1972b6510a5f06f87225854f51900d79b66911431e
ab93e65e846f73d101024dc8b0a23152e3840cb10f89dec2515f50ac05e215820b4fec922f6796b46c247d4113d5f0de4f3014cd94df8bc7dfbf78fc101dee3f
3adcfff9f0debdc3fd1f2d23876a03c74191eaf9b79ffcf9e02efae3e157cfef7f568e9745fcaf3f7cf8cb934fcb81504e53759e7d7ef0dba383675f7cf4fb77
f74be06b02f78bf01e8d8844d7c81edae6111866bce26a4efae27414bd10d322c55a1c481c632da5847f5b850efada18b3343a8e1e4de27af0a680765206bc3c
baed28dc0dc548d112c957c2c8016e72ce9a5c947ae18a965570736f1407e5c2c5a888dbc678b74c760bc74e7cdba304fa6a96968ee1ad90386a6e311c2b1c90
9828a49ff11d424aacbb45a9e3d74dea0b2ef950a15b1435312d75498ff69d6c9a126dd008e2322eb319e2edf866f3266a725666f53ad975915015989528df23
cc71e3653c52382a63d9c3112b3afc2a56619992ddb1f08bb8b65410e980308eda03226519cd7501f616827e0543072b0dfb261b472e5228ba53c6f32ae6bc88
5ce73bad10474919b64be3b0887d4fee408a62b4c555197c93bb15a2ef210e383e36dc372971c2fdf26e7083068e4ad304d14f46a22496970977f2b73b66434c
4cab8126eff4ea88c67fd7b81985ce6d259c5de38656f9eccb07257abfa92d7b0d76afb29ad938d2a88fc31d6dcf2d2e06f4cdefceeb78146f112888d92dea6d
737edb9cbdff7c733eae9ecfbe254fbb3034683d8bd8c1db8ce1d189a7f02165acabc68c5c95661097b0170d3ab0a8f998432ac94f69490897bab241a0830b04
36344870f501556137c4090cf1554f330964ca3a9028e1120e9366b994b7c6c34140d9a3e8a23ea4d84e22b1dae403bbbca097b3b348cec66815980370266841
3338a9b0850b2953b0ed558455b55227965635aa9926e948cb4dd62e36877870796e1a2ce6de842107c168045e5e82d7045a341c7e302303ed771ba32c2c260a
67192219e2014963a4ed9e8d51d50429cb951943b41d3619f4c1f2255e2b48ab6bb6af21ed24412a8aab1d232e8bdeeb4429cbe0699480dbd1726471b138598c
f61a5e7d717ed1433e4e1ade10cecd7019251075a9e74acc02783fe52b61d3fea5c56caa7c1acd7a66985b0455783562fd3e63b0d3071221d53a96a14d0df328
4d01166b4956fff94570eb591950d28d4ea6c5c23224c3bfa605f8d10d2d190e89af8ac12eac68dfd9dbb495f29122a21b0ef6509f8dc43686f0eb54057b0654
c2eb0fd311f40dbcbbd3de368fdce69c165df18d99c1d975cc9210a7ed56976856c9166e1a52ae83b92ba807b695ea6e8c3bbd29a6e4cfc894621affcf4cd1fb
09bc8d5818e808f8f0365960a42ba5e171a1420e5d2809a9df11304898de01d902ef7fe1312415bcd336bf82ecea5f5b739687296b3854aa6d1a2041613f52a1
20640bda92c9be9730aba67b9765c9524626a30aeacac4aadd27bb84f5740f5cd27bbb87424875d34dd236607047f3cfbd4f2ba81fe821a7586f4e27cbf75e5b
03fff4e4638b198c72fbb0196832ffe72ae6e3c17457b5f4863cdb7b8b86e807d331ab965505082b6c05f5b4ec5f5185536eb5b663cd583cbf982907519cb518
16f3812881774a48ff83fd8f0a9fd9af237a43edf16de8ad083e6e6866903690d5e7ece0817483b48b7d189ceca24d26cdcaba361d9db4d7b2cdfa8c27dd5cee
11676bcd4e12ef533a3b1fce5c714e2d9ea5b3530f3bbeb66bc7ba1a227bb4446169981d6c4c60cc97b5e2972fdebf0d815e876f0823a6a44926f88e2530ccd0
5d530750fc56a2215dfd0b0000ffff0300504b0304140006000800000021000dd1909fb60000001b010000270000007468656d652f7468656d652f5f72656c73
2f7468656d654d616e616765722e786d6c2e72656c73848f4d0ac2301484f78277086f6fd3ba109126dd88d0add40384e4350d363f2451eced0dae2c082e8761
be9969bb979dc9136332de3168aa1a083ae995719ac16db8ec8e4052164e89d93b64b060828e6f37ed1567914b284d262452282e3198720e274a939cd08a54f9
80ae38a38f56e422a3a641c8bbd048f7757da0f19b017cc524bd62107bd5001996509affb3fd381a89672f1f165dfe514173d9850528a2c6cce0239baa4c04ca
5bbabac4df000000ffff0300504b01022d0014000600080000002100e9de0fbfff0000001c0200001300000000000000000000000000000000005b436f6e7465
6e745f54797065735d2e786d6c504b01022d0014000600080000002100a5d6a7e7c0000000360100000b00000000000000000000000000300100005f72656c73
2f2e72656c73504b01022d00140006000800000021006b799616830000008a0000001c00000000000000000000000000190200007468656d652f7468656d652f
7468656d654d616e616765722e786d6c504b01022d0014000600080000002100a55e7d2dc7060000d71b00001600000000000000000000000000d60200007468
656d652f7468656d652f7468656d65312e786d6c504b01022d00140006000800000021000dd1909fb60000001b0100002700000000000000000000000000d10900007468656d652f7468656d652f5f72656c732f7468656d654d616e616765722e786d6c2e72656c73504b050600000000050005005d010000cc0a00000000}
{\*\colorschememapping 3c3f786d6c2076657273696f6e3d22312e302220656e636f64696e673d225554462d3822207374616e64616c6f6e653d22796573223f3e0d0a3c613a636c724d
617020786d6c6e733a613d22687474703a2f2f736368656d61732e6f70656e786d6c666f726d6174732e6f72672f64726177696e676d6c2f323030362f6d6169
6e22206267313d226c743122207478313d22646b3122206267323d226c743222207478323d22646b322220616363656e74313d22616363656e74312220616363
656e74323d22616363656e74322220616363656e74333d22616363656e74332220616363656e74343d22616363656e74342220616363656e74353d22616363656e74352220616363656e74363d22616363656e74362220686c696e6b3d22686c696e6b2220666f6c486c696e6b3d22666f6c486c696e6b222f3e}
{\*\latentstyles\lsdstimax267\lsdlockeddef0\lsdsemihiddendef1\lsdunhideuseddef1\lsdqformatdef0\lsdprioritydef99{\lsdlockedexcept \lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority0 \lsdlocked0 Normal;
\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority9 \lsdlocked0 heading 1;\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 2;\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 3;\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 4;
\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 5;\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 6;\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 7;\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 8;\lsdqformat1 \lsdpriority9 \lsdlocked0 heading 9;
\lsdpriority39 \lsdlocked0 toc 1;\lsdpriority39 \lsdlocked0 toc 2;\lsdpriority39 \lsdlocked0 toc 3;\lsdpriority39 \lsdlocked0 toc 4;\lsdpriority39 \lsdlocked0 toc 5;\lsdpriority39 \lsdlocked0 toc 6;\lsdpriority39 \lsdlocked0 toc 7;
\lsdpriority39 \lsdlocked0 toc 8;\lsdpriority39 \lsdlocked0 toc 9;\lsdqformat1 \lsdpriority35 \lsdlocked0 caption;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority10 \lsdlocked0 Title;\lsdpriority1 \lsdlocked0 Default Paragraph Font;
\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority11 \lsdlocked0 Subtitle;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority22 \lsdlocked0 Strong;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority20 \lsdlocked0 Emphasis;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority59 \lsdlocked0 Table Grid;\lsdunhideused0 \lsdlocked0 Placeholder Text;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority1 \lsdlocked0 No Spacing;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority60 \lsdlocked0 Light Shading;\lsdsemihidden0 \lsdunhideused0 \lsdpriority61 \lsdlocked0 Light List;\lsdsemihidden0 \lsdunhideused0 \lsdpriority62 \lsdlocked0 Light Grid;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority63 \lsdlocked0 Medium Shading 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority64 \lsdlocked0 Medium Shading 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority65 \lsdlocked0 Medium List 1;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority66 \lsdlocked0 Medium List 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority67 \lsdlocked0 Medium Grid 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority68 \lsdlocked0 Medium Grid 2;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority69 \lsdlocked0 Medium Grid 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority70 \lsdlocked0 Dark List;\lsdsemihidden0 \lsdunhideused0 \lsdpriority71 \lsdlocked0 Colorful Shading;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority72 \lsdlocked0 Colorful List;\lsdsemihidden0 \lsdunhideused0 \lsdpriority73 \lsdlocked0 Colorful Grid;\lsdsemihidden0 \lsdunhideused0 \lsdpriority60 \lsdlocked0 Light Shading Accent 1;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority61 \lsdlocked0 Light List Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority62 \lsdlocked0 Light Grid Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority63 \lsdlocked0 Medium Shading 1 Accent 1;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority64 \lsdlocked0 Medium Shading 2 Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority65 \lsdlocked0 Medium List 1 Accent 1;\lsdunhideused0 \lsdlocked0 Revision;
\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority34 \lsdlocked0 List Paragraph;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority29 \lsdlocked0 Quote;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority30 \lsdlocked0 Intense Quote;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority66 \lsdlocked0 Medium List 2 Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority67 \lsdlocked0 Medium Grid 1 Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority68 \lsdlocked0 Medium Grid 2 Accent 1;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority69 \lsdlocked0 Medium Grid 3 Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority70 \lsdlocked0 Dark List Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority71 \lsdlocked0 Colorful Shading Accent 1;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority72 \lsdlocked0 Colorful List Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority73 \lsdlocked0 Colorful Grid Accent 1;\lsdsemihidden0 \lsdunhideused0 \lsdpriority60 \lsdlocked0 Light Shading Accent 2;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority61 \lsdlocked0 Light List Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority62 \lsdlocked0 Light Grid Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority63 \lsdlocked0 Medium Shading 1 Accent 2;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority64 \lsdlocked0 Medium Shading 2 Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority65 \lsdlocked0 Medium List 1 Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority66 \lsdlocked0 Medium List 2 Accent 2;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority67 \lsdlocked0 Medium Grid 1 Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority68 \lsdlocked0 Medium Grid 2 Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority69 \lsdlocked0 Medium Grid 3 Accent 2;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority70 \lsdlocked0 Dark List Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority71 \lsdlocked0 Colorful Shading Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority72 \lsdlocked0 Colorful List Accent 2;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority73 \lsdlocked0 Colorful Grid Accent 2;\lsdsemihidden0 \lsdunhideused0 \lsdpriority60 \lsdlocked0 Light Shading Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority61 \lsdlocked0 Light List Accent 3;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority62 \lsdlocked0 Light Grid Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority63 \lsdlocked0 Medium Shading 1 Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority64 \lsdlocked0 Medium Shading 2 Accent 3;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority65 \lsdlocked0 Medium List 1 Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority66 \lsdlocked0 Medium List 2 Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority67 \lsdlocked0 Medium Grid 1 Accent 3;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority68 \lsdlocked0 Medium Grid 2 Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority69 \lsdlocked0 Medium Grid 3 Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority70 \lsdlocked0 Dark List Accent 3;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority71 \lsdlocked0 Colorful Shading Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority72 \lsdlocked0 Colorful List Accent 3;\lsdsemihidden0 \lsdunhideused0 \lsdpriority73 \lsdlocked0 Colorful Grid Accent 3;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority60 \lsdlocked0 Light Shading Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority61 \lsdlocked0 Light List Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority62 \lsdlocked0 Light Grid Accent 4;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority63 \lsdlocked0 Medium Shading 1 Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority64 \lsdlocked0 Medium Shading 2 Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority65 \lsdlocked0 Medium List 1 Accent 4;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority66 \lsdlocked0 Medium List 2 Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority67 \lsdlocked0 Medium Grid 1 Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority68 \lsdlocked0 Medium Grid 2 Accent 4;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority69 \lsdlocked0 Medium Grid 3 Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority70 \lsdlocked0 Dark List Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority71 \lsdlocked0 Colorful Shading Accent 4;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority72 \lsdlocked0 Colorful List Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority73 \lsdlocked0 Colorful Grid Accent 4;\lsdsemihidden0 \lsdunhideused0 \lsdpriority60 \lsdlocked0 Light Shading Accent 5;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority61 \lsdlocked0 Light List Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority62 \lsdlocked0 Light Grid Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority63 \lsdlocked0 Medium Shading 1 Accent 5;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority64 \lsdlocked0 Medium Shading 2 Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority65 \lsdlocked0 Medium List 1 Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority66 \lsdlocked0 Medium List 2 Accent 5;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority67 \lsdlocked0 Medium Grid 1 Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority68 \lsdlocked0 Medium Grid 2 Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority69 \lsdlocked0 Medium Grid 3 Accent 5;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority70 \lsdlocked0 Dark List Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority71 \lsdlocked0 Colorful Shading Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority72 \lsdlocked0 Colorful List Accent 5;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority73 \lsdlocked0 Colorful Grid Accent 5;\lsdsemihidden0 \lsdunhideused0 \lsdpriority60 \lsdlocked0 Light Shading Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority61 \lsdlocked0 Light List Accent 6;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority62 \lsdlocked0 Light Grid Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority63 \lsdlocked0 Medium Shading 1 Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority64 \lsdlocked0 Medium Shading 2 Accent 6;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority65 \lsdlocked0 Medium List 1 Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority66 \lsdlocked0 Medium List 2 Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority67 \lsdlocked0 Medium Grid 1 Accent 6;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority68 \lsdlocked0 Medium Grid 2 Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority69 \lsdlocked0 Medium Grid 3 Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority70 \lsdlocked0 Dark List Accent 6;
\lsdsemihidden0 \lsdunhideused0 \lsdpriority71 \lsdlocked0 Colorful Shading Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority72 \lsdlocked0 Colorful List Accent 6;\lsdsemihidden0 \lsdunhideused0 \lsdpriority73 \lsdlocked0 Colorful Grid Accent 6;
\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority19 \lsdlocked0 Subtle Emphasis;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority21 \lsdlocked0 Intense Emphasis;
\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority31 \lsdlocked0 Subtle Reference;\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority32 \lsdlocked0 Intense Reference;
\lsdsemihidden0 \lsdunhideused0 \lsdqformat1 \lsdpriority33 \lsdlocked0 Book Title;\lsdpriority37 \lsdlocked0 Bibliography;\lsdqformat1 \lsdpriority39 \lsdlocked0 TOC Heading;}}{\*\datastore 010500000200000018000000
4d73786d6c322e534158584d4c5265616465722e362e3000000000000000000000060000
d0cf11e0a1b11ae1000000000000000000000000000000003e000300feff090006000000000000000000000001000000010000000000000000100000feffffff00000000feffffff0000000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
fffffffffffffffffdfffffffeffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
ffffffffffffffffffffffffffffffff52006f006f007400200045006e00740072007900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000016000500ffffffffffffffffffffffff0c6ad98892f1d411a65f0040963251e5000000000000000000000000e0e8
da5d9cb0d201feffffff00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000ffffffffffffffffffffffff00000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000ffffffffffffffffffffffff0000000000000000000000000000000000000000000000000000
000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000ffffffffffffffffffffffff000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000105000000000000}}"), path, ref message, fileHandle);
                }
                if (fileHandle.IsInvalid)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Transaction.Current.Rollback();
            }
            finally
            {
                if (txHandle != null)
                {
                    txHandle.Dispose();
                }
            }
            return fileHandle;
        }

        public static bool CreateFile(string path, ref string message)
        {
            return TransactionActionHelper.DoActionWithCheckOnTransaction((ref string s) =>
            {
                if (!TransactionActionHelper.CheckConditions((ref string mes) =>
                {
                    if (File.Exists(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        mes = "Wrong path or file exists";
                        Transaction.Current.Rollback();
                        return false;
                    }
                    return true;
                }, ref s))
                { 
                    return false;
                }
                var handle = CreateFileHandled(path, ref s);
                if (handle == null)
                {
                    return false;
                }
                handle.Dispose();
                return true;
            }, ref message);
        }

        public static bool CopyFileTo(string path, string pathCopy, ref string message)
        {
            return TransactionActionHelper.DoActionWithCheckOnTransaction((ref string s) =>
            {
                if (!TransactionActionHelper.CheckConditions((ref string mes) =>
                {
                    if (!File.Exists(path) || File.Exists(pathCopy))
                    {
                        mes = "Wrong path or file exists";
                        Transaction.Current.Rollback();
                        return false;
                    }
                    return true;
                }, ref s))
                {
                    return false;
                }
                bool response = false;
                SafeTransactionHandle txHandle = null;
                try
                {
                    IKernelTransaction kernelTx =
                       (IKernelTransaction)TransactionInterop.GetDtcTransaction(Transaction.Current);
                    kernelTx.GetHandle(out txHandle);
                    response = CopyFileTransacted(path, pathCopy, IntPtr.Zero, IntPtr.Zero, false,
                        SafeTransactionHandle.Copy.COPY_FILE_FAIL_IF_EXISTS, txHandle);
                }
                catch (Exception ex)
                {
                    s = ex.Message;
                    Transaction.Current.Rollback();
                }
                finally
                {
                    if (txHandle != null)
                    {
                        txHandle.Dispose();
                    }
                }
                return response;
            }, ref message);
        }

        public static bool DeleteFiles(FileArgs args)
        {
            bool response = true;
            string message = String.Empty;
            return TransactionActionHelper.DoActionWithCheckOnTransaction((ref string s) =>
            {
                foreach (var file in args.Files)
                {
                    if (!response) { return false; }
                    if (!TransactionActionHelper.CheckConditions((ref string mes) =>
                    {
                        if (!File.Exists(file))
                        {
                            mes = "Wrong path or file exists";
                            Transaction.Current.Rollback();
                            return false;
                        }
                        return true;
                    }, ref s))
                    {
                        return false;
                    }
                    SafeTransactionHandle txHandle = null;
                        try
                        {
                            IKernelTransaction kernelTx =
                                (IKernelTransaction)TransactionInterop.GetDtcTransaction(Transaction.Current);
                            kernelTx.GetHandle(out txHandle);
                            if(!DeleteFileTransacted(file, txHandle))
                            {
                                Transaction.Current.Rollback();
                                response = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            response = false;
                            s = ex.Message;
                            Transaction.Current.Rollback();
                        }
                        finally
                        {
                            if (txHandle != null && !txHandle.IsInvalid)
                            {
                                txHandle.Dispose();
                            }
                        }
                }
                return response;
            }, ref message);
        }

        public static bool MoveFile(string path, string pathCopy, ref string message)
        {
            return TransactionActionHelper.DoActionWithCheckOnTransaction((ref string s) =>
            {
                if (!TransactionActionHelper.CheckConditions((ref string mes) =>
                {
                    if (!File.Exists(path) || File.Exists(pathCopy))
                    {
                        mes = "Wrong path or file exists";
                        return false;
                    }
                    return true;
                }, ref s))
                {
                    return false;
                }
                bool response = true;
                SafeTransactionHandle txHandle = null;
                try
                {
                    IKernelTransaction kernelTx =
                       (IKernelTransaction)TransactionInterop.GetDtcTransaction(Transaction.Current);
                    kernelTx.GetHandle(out txHandle);
                    response = MoveFileTransacted(path, pathCopy, IntPtr.Zero, IntPtr.Zero, SafeTransactionHandle.Move.MOVEFILE_COPY_ALLOWED, txHandle);
                }
                catch (Exception ex)
                {
                    response = false;
                    s = ex.Message;
                    Transaction.Current.Rollback();
                }
                finally
                {
                    if (txHandle != null)
                    {
                        txHandle.Dispose();
                    }
                }
                return response;
            }, ref message);
        }

        public static bool WriteToFile(byte[] data, string path, ref string message, SafeFileHandle handle = null)
        {
            if (data == null)
            {
                message = "Empty data";
                return false;
            }
            bool response = true;
            try
            {
                var fileHandle = handle ?? OpenFile(path, new _OFSTRUCT { fFixedDisk = 1, szPathName = path, cBytes = Marshal.SizeOf(typeof(_OFSTRUCT)) }, IntPtr.Zero);
                if (fileHandle == null)
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                using (var stream = new FileStream(fileHandle, FileAccess.Write, 20480, false))
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                response = false;
                message = ex.Message;
                Transaction.Current.Rollback();
            }
            finally
            {
                if (handle != null)
                {
                    handle.Dispose();
                }
            }
            return response;
        }
        protected struct _OFSTRUCT
        {
            public int cBytes;
            public byte fFixedDisk;
            public IntPtr nErrCode;
            public IntPtr Reserved1;
            public IntPtr Reserved2;
            public string szPathName;
        }

        protected sealed class SafeTransactionHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [DllImport("Kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr handle);

            public SafeTransactionHandle() : base(true)
            {
            }

            public SafeTransactionHandle(IntPtr preexistingHandle, bool ownsHandle)
                : base(ownsHandle)
            {
                SetHandle(preexistingHandle);
            }

            public enum Move
            {
                MOVEFILE_COPY_ALLOWED = 0x2,
                MOVEFILE_CREATE_HARDLINK = 0x10,
                MOVEFILE_DELAY_UNTIL_REBOOT = 0x4,
                MOVEFILE_REPLACE_EXISTING = 0x1,
                MOVEFILE_WRITE_THROUGH = 0x8
            }

            public enum Copy
            {
                COPY_FILE_COPY_SYMLINK = unchecked((int)0x80000000),
                COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
                COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
                COPY_FILE_RESTARTABLE = 0x00000002
            }

            public enum FileAccess
            {
                GENERIC_READ = unchecked((int)0x80000000),
                GENERIC_WRITE = 0x40000000
            }

            [Flags]
            public enum FileShare
            {
                FILE_SHARE_NONE = 0x00,
                FILE_SHARE_READ = 0x01,
                FILE_SHARE_WRITE = 0x02,
                FILE_SHARE_DELETE = 0x04
            }

            public enum FileMode
            {
                CREATE_NEW = 1,
                CREATE_ALWAYS = 2,
                OPEN_EXISTING = 3,
                OPEN_ALWAYS = 4,
                TRUNCATE_EXISTING = 5
            }

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }
}
