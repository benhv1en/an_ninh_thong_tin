import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StatusBar,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialIcons as Icon } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import { Button } from '../../components/common/Button';
import { useAuthStore } from '../../store/authStore';
import { useTheme } from '../../theme/ThemeContext';
import { borderRadius, colors, spacing, textStyles } from '../../theme';

type AuthMode = 'login' | 'register';

export const AuthScreen: React.FC = () => {
  const { theme, isDark } = useTheme();
  const account = useAuthStore(state => state.account);
  const error = useAuthStore(state => state.error);
  const isLoading = useAuthStore(state => state.isLoading);
  const login = useAuthStore(state => state.login);
  const register = useAuthStore(state => state.register);
  const clearAuthError = useAuthStore(state => state.clearAuthError);
  const [mode, setMode] = useState<AuthMode>(account ? 'login' : 'register');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState(account?.email || '');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  useEffect(() => {
    setMode(account ? 'login' : 'register');
    setEmail(account?.email || '');
  }, [account]);

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.background.primary,
    },
    scrollContent: {
      flexGrow: 1,
      padding: spacing[5],
      justifyContent: 'center',
    },
    brandBlock: {
      alignItems: 'center',
      marginBottom: spacing[7],
    },
    logo: {
      width: 72,
      height: 72,
      borderRadius: 20,
      alignItems: 'center',
      justifyContent: 'center',
      marginBottom: spacing[4],
    },
    title: {
      ...textStyles.displaySmall,
      color: theme.text.primary,
      textAlign: 'center',
    },
    subtitle: {
      ...textStyles.bodyMedium,
      color: theme.text.secondary,
      textAlign: 'center',
      marginTop: spacing[2],
    },
    form: {
      gap: spacing[3],
    },
    inputGroup: {
      gap: spacing[1],
    },
    label: {
      ...textStyles.labelMedium,
      color: theme.text.secondary,
      textTransform: 'uppercase',
    },
    inputWrapper: {
      flexDirection: 'row',
      alignItems: 'center',
      backgroundColor: theme.surface.secondary,
      borderRadius: borderRadius.lg,
      borderWidth: 1,
      borderColor: theme.border.primary,
      paddingHorizontal: spacing[3],
    },
    input: {
      flex: 1,
      minHeight: 52,
      ...textStyles.bodyMedium,
      color: theme.text.primary,
      paddingHorizontal: spacing[2],
    },
    securityPanel: {
      marginTop: spacing[4],
      padding: spacing[4],
      backgroundColor: theme.surface.secondary,
      borderRadius: borderRadius.lg,
      borderWidth: 1,
      borderColor: theme.border.primary,
      gap: spacing[2],
    },
    securityRow: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: spacing[2],
    },
    securityText: {
      ...textStyles.bodySmall,
      color: theme.text.secondary,
      flex: 1,
    },
    errorBox: {
      marginTop: spacing[3],
      padding: spacing[3],
      borderRadius: borderRadius.lg,
      backgroundColor: colors.expense.bg,
      borderWidth: 1,
      borderColor: colors.expense.light,
    },
    errorText: {
      ...textStyles.bodySmall,
      color: colors.expense.dark,
    },
    action: {
      marginTop: spacing[4],
    },
    footerText: {
      ...textStyles.bodySmall,
      color: theme.text.tertiary,
      textAlign: 'center',
      marginTop: spacing[4],
    },
  }), [theme]);

  const validate = (): boolean => {
    clearAuthError();

    if (!email.trim() || !password) {
      Alert.alert('Thiếu thông tin', 'Vui lòng nhập email và mật khẩu.');
      return false;
    }

    if (mode === 'register') {
      if (!fullName.trim()) {
        Alert.alert('Thiếu thông tin', 'Vui lòng nhập họ tên.');
        return false;
      }

      if (password.length < 8) {
        Alert.alert('Mật khẩu yếu', 'Mật khẩu cần tối thiểu 8 ký tự.');
        return false;
      }

      if (password !== confirmPassword) {
        Alert.alert('Mật khẩu không khớp', 'Vui lòng nhập lại xác nhận mật khẩu.');
        return false;
      }
    }

    return true;
  };

  const handleSubmit = async () => {
    if (!validate()) {
      return;
    }

    const success = mode === 'register'
      ? await register({ fullName, email, password })
      : await login({ email, password });

    if (success) {
      setPassword('');
      setConfirmPassword('');
    }
  };

  const primaryTitle = mode === 'register' ? 'Tạo tài khoản' : 'Đăng nhập';

  return (
    <SafeAreaView style={styles.container}>
      <StatusBar
        barStyle={isDark ? 'light-content' : 'dark-content'}
        backgroundColor={theme.background.primary}
      />
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={{ flex: 1 }}
      >
        <ScrollView contentContainerStyle={styles.scrollContent} keyboardShouldPersistTaps="handled">
          <View style={styles.brandBlock}>
            <LinearGradient colors={theme.gradients.primary} style={styles.logo}>
              <Icon name="lock" size={34} color="#ffffff" />
            </LinearGradient>
            <Text style={styles.title}>CashTrack</Text>
            <Text style={styles.subtitle}>
              {mode === 'register'
                ? 'Tạo tài khoản cục bộ để khóa dữ liệu chi tiêu trên thiết bị.'
                : `Xin chào ${account?.fullName || 'bạn'}, đăng nhập để mở khóa dữ liệu.`}
            </Text>
          </View>

          <View style={styles.form}>
            {mode === 'register' && (
              <View style={styles.inputGroup}>
                <Text style={styles.label}>Họ tên</Text>
                <View style={styles.inputWrapper}>
                  <Icon name="person-outline" size={22} color={theme.text.tertiary} />
                  <TextInput
                    value={fullName}
                    onChangeText={setFullName}
                    style={styles.input}
                    placeholder="Nguyễn Văn A"
                    placeholderTextColor={theme.text.tertiary}
                    autoCapitalize="words"
                  />
                </View>
              </View>
            )}

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Email</Text>
              <View style={styles.inputWrapper}>
                <Icon name="mail-outline" size={22} color={theme.text.tertiary} />
                <TextInput
                  value={email}
                  onChangeText={setEmail}
                  style={styles.input}
                  placeholder="you@example.com"
                  placeholderTextColor={theme.text.tertiary}
                  keyboardType="email-address"
                  autoCapitalize="none"
                  autoCorrect={false}
                  editable={mode === 'register' || !account}
                />
              </View>
            </View>

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Mật khẩu</Text>
              <View style={styles.inputWrapper}>
                <Icon name="key" size={22} color={theme.text.tertiary} />
                <TextInput
                  value={password}
                  onChangeText={setPassword}
                  style={styles.input}
                  placeholder="Tối thiểu 8 ký tự"
                  placeholderTextColor={theme.text.tertiary}
                  secureTextEntry
                  autoCapitalize="none"
                  autoCorrect={false}
                />
              </View>
            </View>

            {mode === 'register' && (
              <View style={styles.inputGroup}>
                <Text style={styles.label}>Xác nhận mật khẩu</Text>
                <View style={styles.inputWrapper}>
                  <Icon name="verified-user" size={22} color={theme.text.tertiary} />
                  <TextInput
                    value={confirmPassword}
                    onChangeText={setConfirmPassword}
                    style={styles.input}
                    placeholder="Nhập lại mật khẩu"
                    placeholderTextColor={theme.text.tertiary}
                    secureTextEntry
                    autoCapitalize="none"
                    autoCorrect={false}
                  />
                </View>
              </View>
            )}
          </View>

          <View style={styles.securityPanel}>
            <View style={styles.securityRow}>
              <Icon name="enhanced-encryption" size={18} color={colors.primary[500]} />
              <Text style={styles.securityText}>AES-256-GCM bảo vệ dữ liệu chi tiêu lưu local.</Text>
            </View>
            <View style={styles.securityRow}>
              <Icon name="vpn-key" size={18} color={colors.info.main} />
              <Text style={styles.securityText}>RSA-OAEP thiết lập khóa phiên gửi Server.</Text>
            </View>
            <View style={styles.securityRow}>
              <Icon name="password" size={18} color={colors.warning.main} />
              <Text style={styles.securityText}>Mật khẩu chỉ lưu dạng bcrypt kèm salt.</Text>
            </View>
          </View>

          {error && (
            <View style={styles.errorBox}>
              <Text style={styles.errorText}>{error}</Text>
            </View>
          )}

          <Button
            title={primaryTitle}
            onPress={handleSubmit}
            loading={isLoading}
            disabled={isLoading}
            size="large"
            fullWidth
            style={styles.action}
            icon={<Icon name={mode === 'register' ? 'person-add' : 'login'} size={20} color="#ffffff" />}
          />

          <Text style={styles.footerText}>
            {mode === 'register'
              ? 'Sau khi tạo tài khoản, khóa mã hóa chỉ được mở trong phiên đăng nhập hiện tại.'
              : 'Đăng xuất sẽ khóa dữ liệu giao dịch trong bộ nhớ app.'}
          </Text>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

export default AuthScreen;
