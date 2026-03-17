import React, { useState, useEffect, useRef, useCallback } from 'react';
import AppHeader from './AppHeader';
import { Dashboard, Uretim, MakineEkle, MakineBilgi, Stok, StokDuzenle, UrunEkle, UrunSil, Personel, PersonelBilgi, PersonelDuzenle, PersonelEkle, SiparisOlustur, Musteriler, MusteriEkle, MusteriBilgi, Muhasebe, GiderDüzenle, AiTahminleri } from 'pages';
import { getThemeClasses } from 'utils/theme';
import { getMusteriler, getMakineler, getPersonel, getUrunler, getGiderListesi, productToStokRow } from 'services';

const MainContent = ({ activeTab, setActiveTab, isDark, toggleTheme, userName, onContentReady }) => {
  const { bgMain } = getThemeClasses(isDark);
  const [personelList, setPersonelList] = useState([]);
  const [selectedPersonId, setSelectedPersonId] = useState(null);
  const [musteriList, setMusteriList] = useState([]);
  const [selectedMusteriId, setSelectedMusteriId] = useState(null);
  const [makineList, setMakineList] = useState([]);
  const [selectedMakineId, setSelectedMakineId] = useState(null);
  const [stokList, setStokList] = useState([]);
  const [giderList, setGiderList] = useState([]);
  const [apiLoading, setApiLoading] = useState(true);
  const [apiError, setApiError] = useState(null);
  const hasReportedReady = useRef(false);

  const loadApi = useCallback(async () => {
    setApiError(null);
    setApiLoading(true);
    try {
      const results = await Promise.allSettled([
        getMusteriler(),
        getMakineler(),
        getPersonel(),
        getUrunler().then((list) => (Array.isArray(list) ? list : []).map(productToStokRow)),
        getGiderListesi(),
      ]);
      const [musteriler, makineler, personel, stok, giderler] = results.map((r) =>
        r.status === 'fulfilled' ? r.value : []
      );
      const failed = results.filter((r) => r.status === 'rejected');
      if (failed.length) {
        const raw = failed.map((r) => r.reason?.message || String(r.reason)).join('; ');
        const isNetworkError = /failed to fetch|network error|load failed/i.test(raw);
        setApiError(
          isNetworkError
            ? 'Backend bağlantısı kurulamadı. Sunucunun çalıştığını ve adresi kontrol edin (.env: REACT_APP_API_BASE_URL).'
            : raw || 'Veriler kısmen yüklenemedi'
        );
      }
      setMusteriList(Array.isArray(musteriler) ? musteriler : []);
      setMakineList(Array.isArray(makineler) ? makineler : []);
      setPersonelList(Array.isArray(personel) ? personel : []);
      setStokList(Array.isArray(stok) ? stok : []);
      setGiderList(Array.isArray(giderler) ? giderler : []);
    } catch (err) {
      const m = err?.message || 'Veriler yüklenemedi';
      setApiError(
        /failed to fetch|network error|load failed/i.test(m)
          ? 'Backend bağlantısı kurulamadı. Sunucunun çalıştığını ve adresi kontrol edin (.env: REACT_APP_API_BASE_URL).'
          : m
      );
      setMusteriList([]);
      setMakineList([]);
      setPersonelList([]);
      setStokList([]);
      setGiderList([]);
    } finally {
      setApiLoading(false);
    }
  }, []);

  useEffect(() => {
    loadApi();
  }, [loadApi]);

  const refreshMusteri = useCallback(() => {
    getMusteriler().then(setMusteriList).catch(() => {});
  }, []);
  const refreshMakine = useCallback(() => {
    getMakineler().then(setMakineList).catch(() => {});
  }, []);
  const refreshPersonel = useCallback(() => {
    getPersonel().then(setPersonelList).catch(() => {});
  }, []);
  const refreshStok = useCallback(() => {
    getUrunler()
      .then((list) => setStokList((Array.isArray(list) ? list : []).map(productToStokRow)))
      .catch(() => setStokList([]));
  }, []);
  const refreshGider = useCallback(() => {
    getGiderListesi().then(setGiderList).catch(() => setGiderList([]));
  }, []);

  useEffect(() => {
    if (!onContentReady || hasReportedReady.current) return;
    const id = requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        hasReportedReady.current = true;
        onContentReady();
      });
    });
    return () => cancelAnimationFrame(id);
  }, [onContentReady]);

  const renderPage = () => {
    switch (activeTab) {
      case 'dashboard':
        return <Dashboard isDark={isDark} onNavigateToAi={() => setActiveTab('ai')} />;
      case 'uretim':
        return (
          <Uretim
            isDark={isDark}
            makineList={makineList}
            onMakineEkle={() => setActiveTab('makine-ekle')}
            onMakineBilgi={(m) => {
              setSelectedMakineId(m.id);
              setActiveTab('makine-bilgi');
            }}
          />
        );
      case 'makine-bilgi': {
        const makine = makineList.find((m) => m.id === selectedMakineId);
        return (
          <MakineBilgi
            isDark={isDark}
            makine={makine}
            onBack={() => {
              setActiveTab('uretim');
              setSelectedMakineId(null);
            }}
          />
        );
      }
      case 'siparis-olustur':
        return (
          <SiparisOlustur
            isDark={isDark}
            musteriList={musteriList}
            onRefreshMusteri={refreshMusteri}
          />
        );
      case 'musteriler':
        return (
          <Musteriler
            isDark={isDark}
            musteriList={musteriList}
            onMusteriEkle={() => setActiveTab('musteriler-ekle')}
            onMusteriBilgi={(m) => {
              setSelectedMusteriId(m.id);
              setActiveTab('musteri-bilgi');
            }}
          />
        );
      case 'musteriler-ekle':
        return (
          <MusteriEkle
            isDark={isDark}
            musteriList={musteriList}
            onRefresh={refreshMusteri}
            onBack={() => setActiveTab('musteriler')}
          />
        );
      case 'musteri-bilgi': {
        const musteri = musteriList.find((m) => m.id === selectedMusteriId);
        return (
          <MusteriBilgi
            isDark={isDark}
            musteri={musteri}
            onBack={() => {
              setActiveTab('musteriler');
              setSelectedMusteriId(null);
            }}
          />
        );
      }
      case 'muhasebe':
        return (
          <Muhasebe
            isDark={isDark}
            giderList={giderList}
            onRefresh={refreshGider}
            onMuhasebeDuzenle={() => setActiveTab('muhasebe-duzenle')}
          />
        );
      case 'muhasebe-duzenle':
        return (
          <GiderDüzenle
            isDark={isDark}
            giderList={giderList}
            onRefresh={refreshGider}
            onBack={() => setActiveTab('muhasebe')}
          />
        );
      case 'makine-ekle':
        return (
          <MakineEkle
            isDark={isDark}
            makineList={makineList}
            onRefresh={refreshMakine}
            onBack={() => setActiveTab('uretim')}
          />
        );
      case 'stok':
        return (
          <Stok
            isDark={isDark}
            stokList={stokList}
            stokLoading={apiLoading}
            onRefresh={refreshStok}
            onDuzenle={() => setActiveTab('stok-duzenle')}
          />
        );
      case 'stok-duzenle':
        return (
          <StokDuzenle
            isDark={isDark}
            onBack={() => setActiveTab('stok')}
            onUrunEkle={() => setActiveTab('urun-ekle')}
            onUrunSil={() => setActiveTab('urun-sil')}
          />
        );
      case 'urun-ekle':
        return <UrunEkle isDark={isDark} makineList={makineList} onBack={() => setActiveTab('stok-duzenle')} onRefresh={refreshStok} />;
      case 'urun-sil':
        return (
          <UrunSil
            isDark={isDark}
            stokList={stokList}
            onBack={() => setActiveTab('stok-duzenle')}
            onRefresh={refreshStok}
          />
        );
      case 'personel':
        return (
          <Personel
            isDark={isDark}
            personelList={personelList}
            onPersonelEkle={() => setActiveTab('personel-ekle')}
            onPersonelBilgi={(p) => {
              setSelectedPersonId(p.id);
              setActiveTab('personel-bilgi');
            }}
          />
        );
      case 'personel-bilgi': {
        const person = personelList.find((p) => p.id === selectedPersonId);
        return (
          <PersonelBilgi
            isDark={isDark}
            person={person}
            onBack={() => {
              setActiveTab('personel');
              setSelectedPersonId(null);
            }}
            onEdit={() => setActiveTab('personel-duzenle')}
          />
        );
      }
      case 'personel-duzenle': {
        const person = personelList.find((p) => p.id === selectedPersonId);
        return (
          <PersonelDuzenle
            isDark={isDark}
            person={person}
            onSave={() => {
              refreshPersonel().then(() => setActiveTab('personel-bilgi'));
            }}
            onBack={() => setActiveTab('personel-bilgi')}
          />
        );
      }
      case 'personel-ekle':
        return (
          <PersonelEkle
            isDark={isDark}
            personelList={personelList}
            onRefresh={refreshPersonel}
            onBack={() => setActiveTab('personel')}
          />
        );
      case 'ai':
        return <AiTahminleri isDark={isDark} />;
      default:
        return <Dashboard isDark={isDark} onNavigateToAi={() => setActiveTab('ai')} />;
    }
  };

  return (
    <div className={`ml-64 p-8 min-h-screen transition-colors duration-300 ${bgMain}`}>
      <div className="max-w-7xl mx-auto">
        <AppHeader
          activeTab={activeTab}
          setActiveTab={setActiveTab}
          isDark={isDark}
          toggleTheme={toggleTheme}
          userName={userName}
        />
        {apiError && (
          <div className="mb-4 px-4 py-3 rounded-lg bg-amber-900/30 border border-amber-700 text-amber-200 text-sm flex flex-wrap items-center gap-3">
            <span className="flex-1 min-w-0">{apiError}</span>
            <button
              type="button"
              onClick={() => {
                setApiError(null);
                loadApi();
              }}
              className="flex-shrink-0 px-3 py-1.5 rounded-md bg-amber-700 hover:bg-amber-600 text-amber-100 text-sm font-medium"
            >
              Yenile
            </button>
          </div>
        )}
        <div key={activeTab} className="page-transition-enter">
          {renderPage()}
        </div>
      </div>
    </div>
  );
};

export default MainContent;
