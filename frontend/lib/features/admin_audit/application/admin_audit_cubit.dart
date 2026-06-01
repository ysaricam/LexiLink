import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_audit/data/admin_action.dart';
import 'package:lexilink_app/features/admin_audit/data/admin_audit_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminAuditStatus { initial, loading, loaded, failure }

class AdminAuditFilter extends Equatable {
  const AdminAuditFilter({
    this.adminUserId,
    this.targetType,
    this.targetId,
  });

  final String? adminUserId;
  final String? targetType;
  final String? targetId;

  AdminAuditFilter copyWith({
    String? adminUserId,
    String? targetType,
    String? targetId,
    bool clearAdminUserId = false,
    bool clearTargetType = false,
    bool clearTargetId = false,
  }) {
    return AdminAuditFilter(
      adminUserId: clearAdminUserId ? null : (adminUserId ?? this.adminUserId),
      targetType: clearTargetType ? null : (targetType ?? this.targetType),
      targetId: clearTargetId ? null : (targetId ?? this.targetId),
    );
  }

  @override
  List<Object?> get props => [adminUserId, targetType, targetId];
}

class AdminAuditCubit extends Cubit<AdminAuditState> {
  AdminAuditCubit({
    required AdminAuditRepository repository,
    int pageSize = 50,
  }) : _repository = repository,
       _pageSize = pageSize,
       super(const AdminAuditState.initial());

  final AdminAuditRepository _repository;
  final int _pageSize;

  Future<void> load() => _fetch(offset: 0, filter: state.filter);

  Future<void> applyFilter(AdminAuditFilter filter) =>
      _fetch(offset: 0, filter: filter);

  Future<void> nextPage() {
    if (!state.hasMore) return Future.value();
    return _fetch(offset: state.offset + _pageSize, filter: state.filter);
  }

  Future<void> prevPage() {
    if (state.offset == 0) return Future.value();
    final next = state.offset - _pageSize;
    return _fetch(offset: next < 0 ? 0 : next, filter: state.filter);
  }

  Future<void> _fetch({
    required int offset,
    required AdminAuditFilter filter,
  }) async {
    emit(
      state.copyWith(
        status: AdminAuditStatus.loading,
        filter: filter,
        clearError: true,
      ),
    );
    try {
      final actions = await _repository.fetch(
        adminUserId: filter.adminUserId,
        targetType: filter.targetType,
        targetId: filter.targetId,
        offset: offset,
        limit: _pageSize,
      );
      emit(
        state.copyWith(
          status: AdminAuditStatus.loaded,
          actions: actions,
          offset: offset,
          hasMore: actions.length == _pageSize,
          filter: filter,
        ),
      );
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminAuditStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }
}

class AdminAuditState extends Equatable {
  const AdminAuditState({
    required this.status,
    required this.actions,
    required this.filter,
    required this.offset,
    required this.hasMore,
    this.errorMessage,
  });

  const AdminAuditState.initial()
    : this(
        status: AdminAuditStatus.initial,
        actions: const [],
        filter: const AdminAuditFilter(),
        offset: 0,
        hasMore: false,
      );

  AdminAuditState copyWith({
    AdminAuditStatus? status,
    List<AdminAction>? actions,
    AdminAuditFilter? filter,
    int? offset,
    bool? hasMore,
    String? errorMessage,
    bool clearError = false,
  }) {
    return AdminAuditState(
      status: status ?? this.status,
      actions: actions ?? this.actions,
      filter: filter ?? this.filter,
      offset: offset ?? this.offset,
      hasMore: hasMore ?? this.hasMore,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminAuditStatus status;
  final List<AdminAction> actions;
  final AdminAuditFilter filter;
  final int offset;
  final bool hasMore;
  final String? errorMessage;

  @override
  List<Object?> get props => [
    status,
    actions,
    filter,
    offset,
    hasMore,
    errorMessage,
  ];
}
